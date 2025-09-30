//
//  ViewController.swift - Excerpts with new implementation
//  ARMetrologyApp
//

import UIKit
import RealityKit
import ARKit
import simd // Required for matrix and vector operations

class ViewController: UIViewController, ARSessionDelegate, CalibrationManagerDelegate {

    // ... (Existing properties and setup methods) ...
    
    // MARK: - Core Button Actions

    // ... (nextPointButtonTapped, resetButtonTapped methods unchanged) ...

    @IBAction func calibrateButtonTapped(_ sender: UIButton) {
        do {
            try calibrationManager.computeTransformation()
            statusLabel.text = "Calibration complete! RMSE: \(String(format: "%.4f", calibrationManager.rootMeanSquareError ?? 0.0))m"
            
            // ** NEW STEP: Visualize the error after successful computation **
            drawErrorVectors() 
            // ** END NEW STEP **
            
            calibrateButton.isEnabled = false // Disable after successful calibration
        } catch let error as CalibrationError {
            statusLabel.text = "Calibration Failed: \(error.localizedDescription)"
            //