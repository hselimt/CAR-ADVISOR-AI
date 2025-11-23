const { app, BrowserWindow, ipcMain } = require('electron');
const path = require('path');
const { spawn } = require('child_process');
const fs = require('fs');
const http = require('http');

let mainWindow;
let apiProcess;

function getResourcePath(resourcePath) {
    if (app.isPackaged) {
        return path.join(process.resourcesPath, 'app.asar.unpacked', resourcePath);
    }
    return path.join(__dirname, resourcePath);
}

function getApiPath() {
    const apiPath = getResourcePath(path.join('api_bin', 'CAR-ADVISOR.exe'));
    return apiPath;
}

function getUiPath() {
    const uiPath = getResourcePath(path.join('ui_build', 'index.html'));
    return uiPath;
}

function waitForServer(url, maxAttempts = 30, interval = 1000) {
    return new Promise((resolve, reject) => {
        let attempts = 0;

        const checkServer = () => {
            attempts++;

            http.get(url, (res) => {
                if (res.statusCode === 200 || res.statusCode === 404) {
                    resolve();
                } else {
                    retry();
                }
            }).on('error', () => {
                retry();
            });
        };

        const retry = () => {
            if (attempts >= maxAttempts) {
                reject(new Error('API server failed to start'));
            } else {
                setTimeout(checkServer, interval);
            }
        };

        checkServer();
    });
}

function startApiServer() {
    const apiPath = getApiPath();
    const workingDir = path.dirname(apiPath);

    apiProcess = spawn(apiPath, [], {
        cwd: workingDir,
        env: { ...process.env }
    });
}

async function createWindow() {
    const uiPath = getUiPath();

    if (!fs.existsSync(uiPath)) {
        app.quit();
        return;
    }

    mainWindow = new BrowserWindow({
        width: 600,
        height: 600,
        resizable: false,
        movable: true,
        center: true,
        show: false,
        frame: false,
        icon: path.join(__dirname, 'hst-high-resolution-logo-transparent.ico'),
        backgroundColor: '#000000',
        autoHideMenuBar: true,
        webPreferences: {
            nodeIntegration: false,
            contextIsolation: true,
            preload: path.join(__dirname, 'preload.js')
        }
    });

    mainWindow.setMenu(null);
    mainWindow.loadFile(uiPath);

    mainWindow.webContents.on('did-fail-load', (event, errorCode, errorDescription) => {
    });

    mainWindow.webContents.on('did-finish-load', () => {
        mainWindow.show();
        mainWindow.focus();
    });

    mainWindow.on('closed', () => {
        mainWindow = null;
    });
}

ipcMain.on('quit-app', () => {
    if (apiProcess) {
        apiProcess.kill();
    }
    app.quit();
});

app.whenReady().then(async () => {
    startApiServer();

    try {
        await waitForServer('http://localhost:5163/api/CarAdvisor');
        await createWindow();
    } catch (error) {
        app.quit();
    }
});

app.on('window-all-closed', () => {
    if (apiProcess) {
        apiProcess.kill();
    }
    if (process.platform !== 'darwin') {
        app.quit();
    }
});

app.on('activate', () => {
    if (mainWindow === null) {
        createWindow();
    }
});

app.on('before-quit', () => {
    if (apiProcess) {
        apiProcess.kill();
    }
});