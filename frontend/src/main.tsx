import React from 'react'
import ReactDOM from 'react-dom/client'
import { MsalProvider } from '@azure/msal-react'
import { PublicClientApplication } from '@azure/msal-browser'
import { msalConfig } from './auth/msalConfig'
import { AuthGate } from './auth/AuthGate'
import './index.css'

async function startApplication() {
  const msalInstance = new PublicClientApplication(msalConfig)
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