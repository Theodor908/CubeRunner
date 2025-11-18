using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectCharacterUI : MonoBehaviour
{
    [SerializeField] private GameObject[] characters;
    [SerializeField] private Button[] characterButtons;
    private bool playerSpawned = false;

    private PlayerSpawner playerSpawner;

    private int selectedCharacterIndex = 0;

    private void Awake()
    {
        playerSpawner = FindAnyObjectByType<PlayerSpawner>();
        if (playerSpawner == null)
        {
            Debug.LogError("PlayerSpawner not found in the scene.");
        }
    }
    private void Start()
    {
        SetupCharacterButtons();
        if (playerSpawner.FoundSpawnPoint() == false)
        {
            return;
        }
        LoadSelectedCharacter();
        UpdateCharacterVisibility();
        playerSpawned = true;
    }

    private void Update()
    {
        if (playerSpawner.FoundSpawnPoint() == false)
        {
            return;
        }
        if (!playerSpawned)
        {
            LoadSelectedCharacter();
            UpdateCharacterVisibility();
            playerSpawned = true;
        }
    }

    private void SetupCharacterButtons()
    {
        for (int i = 0; i < characterButtons.Length; i++)
        {
            int index = i;
            characterButtons[i].onClick.AddListener(() => OnCharacterButtonClicked(index));
        }
    }

    private void OnCharacterButtonClicked(int index)
    {
        selectedCharacterIndex = index;
        SaveSelectedCharacter();
        UpdateCharacterVisibility();
    }

    private void UpdateCharacterVisibility()
    {
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].SetActive(i == selectedCharacterIndex);
        }
    }

    private void SaveSelectedCharacter()
    {
        playerSpawner.SetPlayerPrefab(characters[selectedCharacterIndex]);
        playerSpawner.SpawnPlayer(Quaternion.Euler(0, 90, 0));
        PlayerPrefs.SetInt("SelectedCharacterIndex", selectedCharacterIndex);
        PlayerPrefs.Save();
    }

    private void LoadSelectedCharacter()
    {
        if (PlayerPrefs.HasKey("SelectedCharacterIndex"))
        {
            selectedCharacterIndex = PlayerPrefs.GetInt("SelectedCharacterIndex");
            playerSpawner.SetPlayerPrefab(characters[selectedCharacterIndex]);
            playerSpawner.SpawnPlayer(Quaternion.Euler(0, 90, 0));
        }
    }


}
