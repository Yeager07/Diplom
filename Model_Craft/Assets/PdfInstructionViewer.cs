using UnityEngine;
using UnityPdfViewer;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class PdfInstructionViewer : MonoBehaviour
{
    public string pdfFileName = "manual.pdf";
    private PdfViewerUI pdfViewer;
    private object pdfNavigator;
    private Texture2D[] pageTextures;
    private int currentPageIndex;
    public TextMeshProUGUI pageNumberText;
    public Button nextButton;
    public Button prevButton;

    void Start()
    {
        pdfViewer = GetComponent<PdfViewerUI>();
        if (pdfViewer == null)
        {
            Debug.LogError("PdfViewerUI не найден на объекте!");
            return;
        }

        StartCoroutine(LoadAndSetup());
    }

    IEnumerator LoadAndSetup()
    {
        pdfViewer.LoadPDF(pdfFileName);
        Debug.Log("Загружаем PDF, ожидаем инициализацию...");

        System.Type type = pdfViewer.GetType();
        // Ищем поле navigator (оно публичное)
        FieldInfo navigatorField = type.GetField("navigator", BindingFlags.Public | BindingFlags.Instance);
        if (navigatorField == null)
        {
            Debug.LogError("Поле navigator не найдено!");
            yield break;
        }

        float timeout = 5f;
        float startTime = Time.time;
        object navigator = null;

        while (Time.time - startTime < timeout)
        {
            navigator = navigatorField.GetValue(pdfViewer);
            if (navigator != null)
                break;
            yield return new WaitForSeconds(0.2f);
        }

        if (navigator == null)
        {
            Debug.LogError("navigator не инициализировался за 5 секунд!");
            yield break;
        }

        pdfNavigator = navigator;
        Debug.Log("navigator готов!");

        // Получаем массив текстур страниц
        System.Type navType = pdfNavigator.GetType();
        PropertyInfo pagesProp = navType.GetProperty("Pages");
        if (pagesProp != null)
        {
            pageTextures = (Texture2D[])pagesProp.GetValue(pdfNavigator);
            Debug.Log("Загружено страниц: " + pageTextures.Length);
        }

        // Получаем текущую страницу
        PropertyInfo currentProp = navType.GetProperty("CurrentPage");
        if (currentProp != null)
        currentPageIndex = (int)currentProp.GetValue(pdfNavigator);

        // Отображаем первую страницу
        UpdateDisplay();
    }

    // Обновление состояния кнопок в зависимости от страницы
    private void UpdateButtonStates()
    {
        if (pageTextures == null)
        return;

        if (nextButton != null)
        nextButton.interactable = (currentPageIndex < pageTextures.Length - 1);
    
        if (prevButton != null)
        prevButton.interactable = (currentPageIndex > 0);
    }
    
    // Переход к следующей странице
    public void NextPage()
    {
        if (pdfNavigator == null) return;

        MethodInfo nextMethod = pdfNavigator.GetType().GetMethod("Next");
        nextMethod?.Invoke(pdfNavigator, null);

        RefreshPageInfo();
        UpdateDisplay();
    }

    // Переход к предыдущей странице
    public void PreviousPage()
    {
        if (pdfNavigator == null) return;

        MethodInfo prevMethod = pdfNavigator.GetType().GetMethod("Previous");
        prevMethod?.Invoke(pdfNavigator, null);

        RefreshPageInfo();
        UpdateDisplay();
    }

    // Переход на конкретную страницу (нумерация с 0)
    public void GoToPage(int pageIndex)
    {
        if (pdfNavigator == null) return;

        MethodInfo goToMethod = pdfNavigator.GetType().GetMethod("GoTo");
        goToMethod?.Invoke(pdfNavigator, new object[] { pageIndex });

        RefreshPageInfo();
        UpdateDisplay();
    }

    private void RefreshPageInfo()
    {
        System.Type navType = pdfNavigator.GetType();

        PropertyInfo currentProp = navType.GetProperty("CurrentPage");
        if (currentProp != null)
            currentPageIndex = (int)currentProp.GetValue(pdfNavigator);

        PropertyInfo pagesProp = navType.GetProperty("Pages");
        if (pagesProp != null)
            pageTextures = (Texture2D[])pagesProp.GetValue(pdfNavigator);
    }

    private void UpdateDisplay()
    {
        if (pageTextures != null && currentPageIndex >= 0 && currentPageIndex < pageTextures.Length)
        pdfViewer.pdfImage.texture = pageTextures[currentPageIndex];
        
        if (pageNumberText != null)
        pageNumberText.text = $"{currentPageIndex + 1} / {pageTextures.Length}";

        UpdateButtonStates();
    }

    // Для привязки к кнопкам через инспектор
    public void OnNextButtonClick() => NextPage();
    public void OnPrevButtonClick() => PreviousPage();
}