using UnityEngine;
using UnityPdfViewer;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using SFB;
using System.IO;

public class PdfInstructionViewer : MonoBehaviour
{
    public System.Action<int> OnPageChanged;
    public string pdfFileName = "manual.pdf";
    private PdfViewerUI pdfViewer;
    private object pdfNavigator;
    public Texture2D[] pageTextures;
    public int currentPageIndex;
    public TextMeshProUGUI pageNumberText;
    public Button nextButton;
    public Button prevButton;
    public GameObject loadingScreenPanel;
    public float minLoadTime = 1.5f;
    private float loadStartTime;

    void Start()
    {
        pdfViewer = GetComponent<PdfViewerUI>();
        if(pdfViewer == null)
        {
            Debug.LogError("PdfViewerUI не найден на объекте!");
            return;
        }

        if(loadingScreenPanel != null)
        loadingScreenPanel.SetActive(false);

        if(pdfFileName != "manual.pdf")
        StartCoroutine(LoadAndSetup());
    }

    public void OpenFilePickerAndLoad()
    {
        // Настраиваем фильтры, чтобы показывать только PDF-файлы
        var extensions = new [] {
            new ExtensionFilter("PDF Files", "pdf"),
            new ExtensionFilter("All Files", "*" ),
        };

        // Асинхронно открываем диалог выбора файла
        StandaloneFileBrowser.OpenFilePanelAsync("Выберите файл инструкции", "", extensions, false, (string[] paths) => {
            // Этот код выполнится после того, как пользователь выберет файл
            if(paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
            {
                string selectedPath = paths[0];
                Debug.Log($"Пользователь выбрал файл: {selectedPath}");
                StartCoroutine(CopyAndLoadExternalPdf(selectedPath));
            }

            else
            Debug.Log("Пользователь отменил выбор файла.");
        });
    }

    private IEnumerator CopyAndLoadExternalPdf(string filePath)
    {
        // Показываем сообщение о загрузке (опционально)
        if(pageNumberText != null)
        pageNumberText.text = "Загрузка...";

        // Даём кадр, чтобы текст обновился
        yield return null;

        try
        {
            // 1. Определяем имя файла и путь в StreamingAssets
            string fileName = Path.GetFileName(filePath);
            string destinationPath = Path.Combine(Application.streamingAssetsPath, fileName);

            // 2. Удаляем старый файл с таким же именем, если он есть
            if(File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
                Debug.Log($"Старый файл {fileName} удалён.");
            }

            // 3. Копируем выбранный файл
            File.Copy(filePath, destinationPath);
            Debug.Log($"Файл скопирован в: {destinationPath}");

            // 4. Обновляем имя файла для загрузки
            pdfFileName = fileName;

            // 5. Перезагружаем PDF через существующую логику
            // Останавливаем старую корутину, если она ещё работает, и запускаем новую
            StopAllCoroutines();
            StartCoroutine(LoadAndSetup());

        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка при копировании файла: {e.Message}");
            
            if(pageNumberText != null)
            pageNumberText.text = "Ошибка загрузки";
        }
    }

    public IEnumerator LoadAndSetup()
    {
        if(loadingScreenPanel != null)
        loadingScreenPanel.SetActive(true);


        loadStartTime = Time.time;

        if(pdfViewer.pdfImage != null)
        pdfViewer.pdfImage.texture = null;

        if(pageNumberText != null)
        pageNumberText.text = "Загрузка PDF...";

        pdfViewer.renderDPI = 72;
        
        yield return null;
        Canvas.ForceUpdateCanvases();

        pdfViewer.LoadPDF(pdfFileName);
        Debug.Log("Загружаем PDF, ожидаем инициализацию...");

        System.Type type = pdfViewer.GetType();
        FieldInfo navigatorField = type.GetField("navigator", BindingFlags.Public | BindingFlags.Instance);
        
        if(navigatorField == null)
        {
            Debug.LogError("Поле navigator не найдено!");
            yield break;
        }

        float timeout = 5f;
        float startTime = Time.time;
        object navigator = null;

        while(Time.time - startTime < timeout)
        {
            navigator = navigatorField.GetValue(pdfViewer);
            
            if(navigator != null)
            break;
            
            yield return new WaitForSeconds(0.2f);
        }

        if(navigator == null)
        {
            Debug.LogError("navigator не инициализировался за 5 секунд!");
            yield break;
        }

        pdfNavigator = navigator;
        Debug.Log("navigator готов!");

        // Получаем массив текстур страниц
        System.Type navType = pdfNavigator.GetType();
        PropertyInfo pagesProp = navType.GetProperty("Pages");
        
        if(pagesProp != null)
        {
            pageTextures = (Texture2D[])pagesProp.GetValue(pdfNavigator);
            Debug.Log("Загружено страниц: " + pageTextures.Length);
        }

        // Получаем текущую страницу
        PropertyInfo currentProp = navType.GetProperty("CurrentPage");
        
        if(currentProp != null)
        currentPageIndex = (int)currentProp.GetValue(pdfNavigator);

        float elapsed = Time.time - loadStartTime;

        if(elapsed < minLoadTime)
        yield return new WaitForSeconds(minLoadTime - elapsed);

        if(loadingScreenPanel != null)
        loadingScreenPanel.SetActive(false);

        // Отображаем первую страницу
        UpdateDisplay();

        InvokePageChanged(currentPageIndex + 1);
    }

    // Обновление состояния кнопок в зависимости от страницы
    private void UpdateButtonStates()
    {
        if(pageTextures == null)
        return;

        if(nextButton != null)
        nextButton.interactable = (currentPageIndex < pageTextures.Length - 1);
    
        if(prevButton != null)
        prevButton.interactable = (currentPageIndex > 0);
    }
    
    // Переход к следующей странице
    public void NextPage()
    {
        if(pdfNavigator == null)
        return;

        MethodInfo nextMethod = pdfNavigator.GetType().GetMethod("Next");
        nextMethod?.Invoke(pdfNavigator, null);

        RefreshPageInfo();
        UpdateDisplay();
        
        InvokePageChanged(currentPageIndex + 1);
    }

    // Переход к предыдущей странице
    public void PreviousPage()
    {
        if (pdfNavigator == null) return;

        MethodInfo prevMethod = pdfNavigator.GetType().GetMethod("Previous");
        prevMethod?.Invoke(pdfNavigator, null);

        RefreshPageInfo();
        UpdateDisplay();
        
        InvokePageChanged(currentPageIndex + 1);
    }

    // Переход на конкретную страницу (нумерация с 0)
    public void GoToPage(int pageIndex)
    {
        if(pdfNavigator == null)
        return;

        MethodInfo goToMethod = pdfNavigator.GetType().GetMethod("GoTo");
        goToMethod?.Invoke(pdfNavigator, new object[] { pageIndex });

        RefreshPageInfo();
        UpdateDisplay();

        InvokePageChanged(currentPageIndex + 1);
    }

    private void RefreshPageInfo()
    {
        System.Type navType = pdfNavigator.GetType();

        PropertyInfo currentProp = navType.GetProperty("CurrentPage");
        
        if(currentProp != null)
        currentPageIndex = (int)currentProp.GetValue(pdfNavigator);

        PropertyInfo pagesProp = navType.GetProperty("Pages");
        
        if(pagesProp != null)
        pageTextures = (Texture2D[])pagesProp.GetValue(pdfNavigator);
    }

    private void UpdateDisplay()
    {
        if(pageTextures != null && currentPageIndex >= 0 && currentPageIndex < pageTextures.Length)
        pdfViewer.pdfImage.texture = pageTextures[currentPageIndex];
        
        if(pageNumberText != null)
        pageNumberText.text = $"{currentPageIndex + 1} / {pageTextures.Length}";

        UpdateButtonStates();
    }

    private void InvokePageChanged(int page)
    {
        // Копируем список делегатов, чтобы избежать изменений во время итерации
        var delegates = OnPageChanged?.GetInvocationList();
        
        if(delegates == null)
        return;

        foreach(var del in delegates)
        {
            // Проверяем, жив ли целевой объект
            if(del.Target != null && del.Target.Equals(null))
            continue;
            
            try
            {
                del.DynamicInvoke(page);
            }
            catch
            {
                Debug.LogWarning($"Ошибка при вызове обработчика перелистывания: {del.Method.Name}");
            }
        }
    }

    // Для привязки к кнопкам через инспектор
    public void OnNextButtonClick() => NextPage();
    public void OnPrevButtonClick() => PreviousPage();
}