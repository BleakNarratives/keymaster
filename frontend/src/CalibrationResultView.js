import React from 'react';

function CalibrationResultView({ result }) {
  if (!result) return null;
  return (
    <div>
      <h2>Calibration Result</h2>
      <p><strong>RMSE:</strong> {result.rmse}</p>
      <p><strong>Source Platform:</strong> {result.source_platform}</p>
      <p><strong>Number of Points Used:</strong> {result.num_points_used}</p>
      <h3>Transformation Matrix (4x4):</h3>
      <table border="1">
        <tbody>
          {[0,1,2,3].map(row => (
            <tr key={row}>
              {[0,1,2,3].map(col => (
                <td key={col}>{result.transform_elements[row*4+col]}</td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export default CalibrationResultView;
