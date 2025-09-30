import React, { useState } from 'react';
import CalibrationUpload from './CalibrationUpload';
import CalibrationResultView from './CalibrationResultView';
import AnalysisForm from './AnalysisForm';
import LoginForm from './LoginForm';

function App() {
  const [result, setResult] = useState(null);
  const [user, setUser] = useState(null);
  if (!user) {
    return <LoginForm onLogin={setUser} />;
  }
  return (
    <div className="App">
      <h1>KeyMaster Calibration Dashboard</h1>
      <CalibrationUpload onResult={setResult} />
      <CalibrationResultView result={result} />
      <AnalysisForm calibrationResult={result} />
    </div>
  );
}

export default App;
