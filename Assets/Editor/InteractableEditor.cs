using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Interactable))]
public class InteractableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Interactable interactable = (Interactable)target;

        // Always show the type selector
        EditorGUILayout.PropertyField(serializedObject.FindProperty("type"));

        EditorGUILayout.Space();

        // Show settings based on selected type
        switch (interactable.type)
        {
            case InteractableType.Door:
                ShowDoorSettings();
                break;

            case InteractableType.Lantern:
                ShowLanternSettings();
                break;

            case InteractableType.Window:
                ShowWindowSettings();
                break;

            case InteractableType.Photo:
                ShowPhotoSettings();
                break;

            case InteractableType.Crowbar:
                ShowCrowbarSettings();
                break;

            case InteractableType.BathroomSink:
                ShowBathroomSinkSettings();
                break;

            case InteractableType.FinalDoor:
                ShowFinalDoorSettings();
                break;

            case InteractableType.Gun:
                ShowGunSettings();
                break;

            case InteractableType.Generic:
                EditorGUILayout.HelpBox("Generic interactable. Select a specific type to see settings.", MessageType.Info);
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }

    void ShowDoorSettings()
    {
        EditorGUILayout.LabelField("Door Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("doorTarget"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("postHouseSwitchTarget"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("postCrowbarPickupTarget"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("oneTimeUse"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("startLocked"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("doorToEnableOnLock"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("doorToLockOnUse"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("requiresLantern"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("lockedMessageTrigger"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("doorOpenSound"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("doorCloseSound"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Jammed Door Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("isJammedDoor"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("jammedDoorSound"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("unjamDoorSound"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("delayedJammedAudio"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("objectToDeleteOnJammed"));
    }

    void ShowLanternSettings()
    {
        EditorGUILayout.LabelField("Lantern Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("lanternPickupSound"));
    }

    void ShowWindowSettings()
    {
        EditorGUILayout.LabelField("Window Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("windowLockSound"));
    }

    void ShowPhotoSettings()
    {
        EditorGUILayout.LabelField("Photo Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("photoUIImage"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("photoDialogueTrigger"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("photoFadeInDuration"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("photoDisplayDuration"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("photoFadeOutDuration"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("photoPickupSound"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("photoDisplaySound"));
    }

    void ShowCrowbarSettings()
    {
        EditorGUILayout.LabelField("Crowbar Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("crowbarPickupTrigger"));
    }

    void ShowBathroomSinkSettings()
    {
        EditorGUILayout.LabelField("Bathroom Sink Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("faucetOnSound"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("waterSplashingSound"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("faucetOffSound"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("faucetOnDuration"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("splashingDuration"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("faucetOffDuration"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("requiresCrowbar"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("lockedMessageTrigger"));
    }

    void ShowFinalDoorSettings()
    {
        EditorGUILayout.LabelField("Final Door Settings", EditorStyles.boldLabel);

        SerializedProperty prop;

        prop = serializedObject.FindProperty("objectsToAppear");
        if (prop != null) EditorGUILayout.PropertyField(prop, true);

        prop = serializedObject.FindProperty("objectsToDisappear");
        if (prop != null) EditorGUILayout.PropertyField(prop, true);

        prop = serializedObject.FindProperty("speedMultiplier");
        if (prop != null) EditorGUILayout.PropertyField(prop);

        prop = serializedObject.FindProperty("doorRotationDegrees");
        if (prop != null) EditorGUILayout.PropertyField(prop);

        prop = serializedObject.FindProperty("doorRotationSpeed");
        if (prop != null) EditorGUILayout.PropertyField(prop);

        prop = serializedObject.FindProperty("doorPivotPoint");
        if (prop != null) EditorGUILayout.PropertyField(prop);

        prop = serializedObject.FindProperty("timothyObject");
        if (prop != null) EditorGUILayout.PropertyField(prop);

        prop = serializedObject.FindProperty("timothyMoveSpeed");
        if (prop != null) EditorGUILayout.PropertyField(prop);

        prop = serializedObject.FindProperty("timothyActivationDistance");
        if (prop != null) EditorGUILayout.PropertyField(prop);

        prop = serializedObject.FindProperty("finalDoorAudio");
        if (prop != null) EditorGUILayout.PropertyField(prop);

        prop = serializedObject.FindProperty("chaseMusic");
        if (prop != null) EditorGUILayout.PropertyField(prop);

        prop = serializedObject.FindProperty("timothyKillSound");
        if (prop != null) EditorGUILayout.PropertyField(prop);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Light Settings", EditorStyles.boldLabel);

        prop = serializedObject.FindProperty("playerLight");
        if (prop != null) EditorGUILayout.PropertyField(prop);

        prop = serializedObject.FindProperty("targetLightRange");
        if (prop != null) EditorGUILayout.PropertyField(prop);

        prop = serializedObject.FindProperty("lightChangeDuration");
        if (prop != null) EditorGUILayout.PropertyField(prop);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Player Movement", EditorStyles.boldLabel);

        prop = serializedObject.FindProperty("playerBackAwayDistance");
        if (prop != null) EditorGUILayout.PropertyField(prop);

        prop = serializedObject.FindProperty("playerBackAwaySpeed");
        if (prop != null) EditorGUILayout.PropertyField(prop);
    }

    void ShowGunSettings()
    {
        EditorGUILayout.LabelField("Gun Settings", EditorStyles.boldLabel);

        SerializedProperty prop;

        prop = serializedObject.FindProperty("gunPickupSound");
        if (prop != null) EditorGUILayout.PropertyField(prop);

        prop = serializedObject.FindProperty("gunShootSound");
        if (prop != null) EditorGUILayout.PropertyField(prop);

        prop = serializedObject.FindProperty("gunUIImage");
        if (prop != null) EditorGUILayout.PropertyField(prop);

        prop = serializedObject.FindProperty("shotEndingBlackImage");
        if (prop != null) EditorGUILayout.PropertyField(prop);
    }
}