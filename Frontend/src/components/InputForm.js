import React, { useState } from 'react';
import './InputForm.css';

function InputForm({ onStartAnalysis }) {
    const [budget, setBudget] = useState('');
    const [preferences, setPreferences] = useState('');

    const handleSubmit = (e) => {
        e.preventDefault();

        if (!budget || !preferences) {
            alert('Please Fill The Blank Areas');
            return;
        }

        onStartAnalysis(budget, preferences);
    };

    return (
        <div className="input-form-container">
            <div className="input-form-card">
                <h2>Car Choices</h2>
                <form onSubmit={handleSubmit}>
                    <div className="form-group">
                        <label>Budget</label>
                        <input
                            type="text"
                            value={budget}
                            onChange={(e) => setBudget(e.target.value)}
                            placeholder="E.g. 1500000"
                            className="form-input"
                        />
                    </div>

                    <div className="form-group">
                        <label>Needs and Preferences</label>
                        <textarea
                            value={preferences}
                            onChange={(e) => setPreferences(e.target.value)}
                            placeholder="E.g. Sport car, not electric. Should have at least 200 hp and an engine size of 1.6 liters or more.
                                The exterior should look aggressive, the interior should be luxurious with leather seats and a large display. 
                                It should be comfortable for both city and highway driving. Performance is the priority, not fuel consumption."
                            className="form-textarea"
                            rows="4"
                        />
                    </div>

                    <button type="submit" className="submit-button">
                        Start Analize
                    </button>
                </form>

                <div className="info-box">
                    <p>6 + 1 Agents Will Debate For The Car That Suits You Most And Give Suggestions</p>
                </div>
            </div>
        </div>
    );
}

export default InputForm;