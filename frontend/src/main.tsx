import React from 'react'
import ReactDOM from 'react-dom/client'
import { MsalProvider } from '@azure/msal-react'
import { msalInstance } from './auth/msalConfig'
import { AuthGate } from './auth/AuthGate'
import './index.css'

async function startApplication() {
  await msalInstance.initialize()

  ReactDOM.createRoot(
    document.getElementById('root')!
  ).render(
    <React.StrictMode>
      <MsalProvider instance={msalInstance}>
        <AuthGate />
      </MsalProvider>
    </React.StrictMode>
  )
}

void startApplication()