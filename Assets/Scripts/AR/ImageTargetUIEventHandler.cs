/*============================================================================== 
 * Copyright (c) 2012-2014 Qualcomm Connected Experiences, Inc. All Rights Reserved. 
 * ==============================================================================*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vuforia;

/// <summary>
/// UI Event Handler class that handles events generated by user-tap actions
/// over the UI Options Menu
/// </summary>
public class ImageTargetUIEventHandler : ISampleAppUIEventHandler { 
    
    #region PUBLIC_MEMBER_VARIABLES
    public override event System.Action CloseView;
    public override event System.Action GoToAboutPage;
    #endregion PUBLIC_MEMBER_VARIABLES
    
    #region PRIVATE_MEMBER_VARIABLES
    private static bool sExtendedTrackingIsEnabled;
    private ImageTargetUIView mView;
    private bool mCameraFacingFront;
    #endregion PRIVATE_MEMBER_VARIABLES
    
    #region PUBLIC_MEMBER_PROPERTIES
    public ImageTargetUIView View
    {
        get {
            if(mView == null){
                mView = new ImageTargetUIView();
                mView.LoadView();
            }
            return mView;
        }
    }

    /// <summary>
    /// Currently, there is no mechanism to query the SDK to know whether or not extended tracking is enabled/disabled.
    /// Therefore, it needs to be handled at the app layer.
    /// </value>
    public static bool ExtendedTrackingIsEnabled
    {
        get {
            return sExtendedTrackingIsEnabled;
        }
    }

    #endregion PUBLIC_MEMBER_PROPERTIES
    
    #region PUBLIC_METHODS
    public override void UpdateView (bool tf)
    {
        this.View.UpdateUI(tf);
    }
    
    public override  void Bind()
    {
        this.View.mExtendedTracking.TappedOn    += OnTappedToTurnOnTraking;
        this.View.mCameraFlashSettings.TappedOn += OnTappedToTurnOnFlash;
        this.View.mAutoFocusSetting.TappedOn    += OnTappedToTurnOnAutoFocus;
        this.View.mCameraFacing.TappedOnOption  += OnTappedToTurnCameraFacing;
        this.View.mDataSet.TappedOnOption       += OnTappedOnDataSet;
        this.View.mCloseButton.TappedOn         += OnTappedOnCloseButton;
        this.View.mAboutLabel.TappedOn          += OnTappedOnAboutButton;

        // register Vuforia started callback
        VuforiaAbstractBehaviour VuforiaBehaviour = (VuforiaAbstractBehaviour)FindObjectOfType(typeof(VuforiaAbstractBehaviour));
        if (VuforiaBehaviour)
        { 
            VuforiaBehaviour.RegisterVuforiaStartedCallback(EnableContinuousAutoFocus);
            VuforiaBehaviour.RegisterOnPauseCallback(OnPause);
        }
    }
    
    public override  void UnBind()
    { 
        this.View.mExtendedTracking.TappedOn    -= OnTappedToTurnOnTraking;
        this.View.mCameraFlashSettings.TappedOn -= OnTappedToTurnOnFlash;
        this.View.mAutoFocusSetting.TappedOn    -= OnTappedToTurnOnAutoFocus;
        this.View.mCameraFacing.TappedOnOption  -= OnTappedToTurnCameraFacing;
        this.View.mDataSet.TappedOnOption       -= OnTappedOnDataSet;
        this.View.mCloseButton.TappedOn         -= OnTappedOnCloseButton;
        this.View.mAboutLabel.TappedOn          -= OnTappedOnAboutButton;
        sExtendedTrackingIsEnabled = false;

        // unregister Vuforia started callback
        VuforiaAbstractBehaviour VuforiaBehaviour = (VuforiaAbstractBehaviour)FindObjectOfType(typeof(VuforiaAbstractBehaviour));
        if (VuforiaBehaviour)
        {
            VuforiaBehaviour.UnregisterVuforiaStartedCallback(EnableContinuousAutoFocus);
            VuforiaBehaviour.UnregisterOnPauseCallback(OnPause);
        }

        this.View.UnLoadView();
        mView = null;
    }
    
    public override  void TriggerAutoFocus()
    {
        StartCoroutine(TriggerAutoFocusAndEnableContinuousFocusIfSet());
    }
    
    #endregion PUBLIC_METHODS
    
    #region PRIVATE_METHODS
    
    /// <summary>
    /// Activating trigger autofocus mode unsets continuous focus mode (if was previously enabled from the UI Options Menu)
    /// So, we wait for a second and turn continuous focus back on (if options menu shows as enabled)
    /// </returns>
    private IEnumerator TriggerAutoFocusAndEnableContinuousFocusIfSet()
    {
        //triggers a single autofocus operation 
        if (CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_TRIGGERAUTO)) {
              this.View.FocusMode = CameraDevice.FocusMode.FOCUS_MODE_TRIGGERAUTO;
        }
        
        yield return new WaitForSeconds(1.0f);
         
        //continuous focus mode is turned back on if it was previously enabled from the options menu
        if(this.View.mAutoFocusSetting.IsEnabled)
        {
            if (CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO)) {
              this.View.FocusMode = CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO;
            }
        }
        
        Debug.Log (this.View.FocusMode);
        
    }

    private void OnPause(bool pause)
    {
        if (!pause && this.View.mAutoFocusSetting.IsEnabled)
        {
            // set to continous autofocus
            CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
        }

        //On hitting the home button, the app tends to turn off the flash
        //So, setting the UI to reflect that
        this.View.mCameraFlashSettings.Enable(pause);
    }

    private void OnTappedOnAboutButton(bool tf)
    {
        if(this.GoToAboutPage != null)
        {
            this.GoToAboutPage();
        }
    }
    
    private void OnTappedToTurnOnTraking(bool tf)
    {
        if(!ExtendedTracking(tf))
        {
            this.View.mExtendedTracking.Enable(false);
            ImageTargetUIEventHandler.sExtendedTrackingIsEnabled = false;
        }
        else 
        {
            ImageTargetUIEventHandler.sExtendedTrackingIsEnabled = tf;
            this.View.mExtendedTracking.Enable(tf);
            // to better demostrate the effect, we switch the augmentation models - a teapot is used for normal tracking,
            // a skyscraper for extended tracking.
            SwitchModels(tf);
        }
        OnTappedToClose();
    }
    
    private void OnTappedToTurnOnFlash(bool tf)
    {
        if(tf)
        {
            if(!CameraDevice.Instance.SetFlashTorchMode(true) || mCameraFacingFront)
            {
                this.View.mCameraFlashSettings.Enable(false);
            }
        }
        else 
        {
            CameraDevice.Instance.SetFlashTorchMode(false);
        }
        
        OnTappedToClose();
    }

    //We want autofocus to be enabled when the app starts
    private void EnableContinuousAutoFocus()
    {
        if (CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO))
        {
            this.View.FocusMode = CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO;
            this.View.mAutoFocusSetting.Enable(true);
        }
    }
    
    private void OnTappedToTurnOnAutoFocus(bool tf)
    {
        if(tf)
        {
            if (CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO))
            {
                this.View.FocusMode = CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO;
            }
            else 
            {
                this.View.mAutoFocusSetting.Enable(false);
            }
        }
        else 
        {
            if (CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_NORMAL))
            {
                this.View.FocusMode = CameraDevice.FocusMode.FOCUS_MODE_NORMAL;
            }
        }
        
        OnTappedToClose();
    }
    
    private void OnTappedToTurnCameraFacing(int val)
    {
        if(val == 0)
        {
            //internally, flash is always turned off everytime it tries to switch to front camera
            //so updating the UI options to reflect that.
            this.View.mCameraFlashSettings.Enable(false);

            if(ChangeCameraDirection(CameraDevice.CameraDirection.CAMERA_FRONT)) {
                mCameraFacingFront = true;
            }
            else {
                ChangeCameraDirection(CameraDevice.CameraDirection.CAMERA_BACK);
                mCameraFacingFront = false;
                this.View.mCameraFacing.EnableIndex(1);
            }
        }
        else 
        {
            ChangeCameraDirection(CameraDevice.CameraDirection.CAMERA_BACK);
            mCameraFacingFront = false;
        }
        
        OnTappedToClose();
    }

    private bool stopRunningObjectTracker()
    {
        bool needsObjectTrackerRestart = false;

        ObjectTracker objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        if (objectTracker != null)
        {
            if (objectTracker.IsActive)
            {
                objectTracker.Stop();
                needsObjectTrackerRestart = true;
            }
        }
        return needsObjectTrackerRestart;
    }

    private bool restartRunningObjectTracker()
    {
        bool hasObjectTrackerRestarted = false;

        ObjectTracker objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        if (objectTracker != null)
        {
            if (!objectTracker.IsActive)
            {
                hasObjectTrackerRestarted = objectTracker.Start();
            }
        }
        return hasObjectTrackerRestarted;
    }

    private void ResetCameraFacingToBack()
    {
        bool needsObjectTrackerRestart = stopRunningObjectTracker();

        CameraDevice.Instance.Stop();
        CameraDevice.Instance.Init(CameraDevice.CameraDirection.CAMERA_BACK);
        CameraDevice.Instance.Start();
        mCameraFacingFront = false;

        if (needsObjectTrackerRestart)
            restartRunningObjectTracker();

    }
    
    private bool ChangeCameraDirection(CameraDevice.CameraDirection direction)
    {
        bool directionSupported = false;

        bool needsObjectTrackerRestart = stopRunningObjectTracker();

        CameraDevice.Instance.Stop();
        CameraDevice.Instance.Deinit();

        if(CameraDevice.Instance.Init(direction)) {
            directionSupported = true;
        }
        CameraDevice.Instance.Start();

        if (needsObjectTrackerRestart)
            restartRunningObjectTracker();

        return directionSupported;
    }
    
    private void OnTappedOnDataSet(int val)
    {
        if(val == 0)
        {
            ActivateDataSet("Wall");
        }
        else 
        {
			ActivateDataSet("AtriumEdit");
        }
        
        OnTappedToClose();
    }
    
    private void OnTappedToClose()
    {
        if(this.CloseView != null)
        {
            this.CloseView();
        }
    }
    
    private void OnTappedOnCloseButton()
    {
        OnTappedToClose();
    }
    
    private void ActivateDataSet(string datasetPath)
    {
        //ObjectTracker tracks ImageTargets contained in a DataSet and provides methods for creating, activating and deactivating datasets.
        ObjectTracker objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        IEnumerable<DataSet> datasets = objectTracker.GetDataSets();

        IEnumerable<DataSet> activeDataSets = objectTracker.GetActiveDataSets();
        List<DataSet> activeDataSetsToBeRemoved = activeDataSets.ToList();
        
        //1. Loop through all the active datasets and deactivate them.
        foreach(DataSet ads in activeDataSetsToBeRemoved) {
            objectTracker.DeactivateDataSet(ads);
        }

        //Swapping of the datasets should not be done while the ObjectTracker is working at the same time.
        //2. So, Stop the tracker first.
        objectTracker.Stop();
        
        //3. Then, look up the new dataset and if one exists, activate it.
        foreach(DataSet ds in datasets) {
            if(ds.Path.Contains(datasetPath)) {
                objectTracker.ActivateDataSet(ds);
            }
        }
        
        //4. Finally, start the object tracker.
        objectTracker.Start();
    }
    
    private void SwitchModels(bool tf)
    {
        ImageTargetTrackableEventHandler[] trackableEventHandlers = GameObject.FindObjectsOfType(typeof(ImageTargetTrackableEventHandler)) as ImageTargetTrackableEventHandler[];
        foreach(ImageTargetTrackableEventHandler handler in trackableEventHandlers)
        {
            if(handler.isBeingTracked)
            {
                Renderer[] rendererComponents = handler.GetComponentsInChildren<Renderer>();
                Collider[] colliderComponents = handler.GetComponentsInChildren<Collider>();
                
                foreach (Renderer component in rendererComponents)
                {
                    if(component.gameObject.name == "tower")
                             component.enabled = tf;
                    if(component.gameObject.name == "teapot")
                        component.enabled = !tf;
                }
        
                foreach (Collider component in colliderComponents)
                {
                    if(component.gameObject.name == "tower")
                             component.enabled = tf;
                    if(component.gameObject.name == "teapot")
                        component.enabled = !tf;
                }
            }
        }
    }
    
    /// <summary>
    /// This method turns extended tracking on or off for all currently available targets.
    /// Extended tracking allows to track targets when they are not in view.
    /// Returns false if extended tracking is not supported on the device and true otherwise.
    /// </summary>
    private bool ExtendedTracking(bool tf)
    {
        // the StateManager gives access to all available TrackableBehavours
        StateManager stateManager = TrackerManager.Instance.GetStateManager();
        // We iterate over all TrackableBehaviours to start or stop extended tracking for the targets they represent.

        bool extendedTrackingStateChanged = true;
        foreach(var behaviour in stateManager.GetTrackableBehaviours())
        {
            var imageBehaviour = behaviour as ImageTargetBehaviour;
            if(imageBehaviour != null)
            {
                if(tf) {
                    //only if extended tracking is supported
                    if(!imageBehaviour.ImageTarget.StartExtendedTracking()) {
                        extendedTrackingStateChanged = false;
                    }
                }
                else {
                    if(!imageBehaviour.ImageTarget.StopExtendedTracking()) {
                        extendedTrackingStateChanged = false;
                    }
                }
            }
        }

        if(!extendedTrackingStateChanged) {
            Debug.LogWarning("Extended Tracking Failed!");
        }

        return extendedTrackingStateChanged;
    }
    #endregion PRIVATE_METHODS
}

