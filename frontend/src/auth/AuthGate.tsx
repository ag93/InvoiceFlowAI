import { useState } from 'react'
import { useIsAuthenticated, useMsal } from '@azure/msal-react'
import { InteractionStatus } from '@azure/msal-browser'
import { invoiceApiScopes } from './msalConfig'
import App from '../App'

export function AuthGate() {
  const { instance, accounts, inProgress } = useMsal()
  const isAuthenticated = useIsAuthenticated()
  const [error, setError] = useState<string | null>(null)

  const isBusy = inProgress !== InteractionStatus.None

  async function handleSignIn() {
    setError(null)

    try {
      await instance.loginRedirect({scopes: invoiceApiScopes,})
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Sign-in failed.'
      )
    }
  }

  async function handleSignOut() {
    setError(null)

    try {
      await instance.logoutRedirect({
        account: instance.getActiveAccount() ?? accounts[0],
      })
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Sign-out failed.'
      )
    }
  }

  if (isBusy) {
    return (
      <main className="login-page">
        <p role="status">Completing Microsoft sign-in...</p>
      </main>
    )
  }

  if (!isAuthenticated) {
    return (
      <main className="login-page">
        <section className="login-card">
          <div className="brand-mark">IF</div>

          <h1>InvoiceFlow AI</h1>

          <p>
            Sign in with your Microsoft account
            to manage invoices.
          </p>

          <button
            type="button"
            className="upload-button"
            onClick={() => void handleSignIn()}
          >
            Sign in with Microsoft
          </button>

          {error && (
            <p className="upload-error" role="alert">
              {error}
            </p>
          )}
        </section>
      </main>
    )
  }

  const account = instance.getActiveAccount() ?? accounts[0]

  return (
    <>
      <div className="auth-toolbar">
        <span>
          Signed in as{' '}
          <strong>
            {account?.name ?? account?.username ?? 'Microsoft user'}
          </strong>
        </span>

        <button
          type="button"
          className="signout-button"
          onClick={() => void handleSignOut()}
        >
          Sign out
        </button>

        <button
          type="button"
          className="signout-button"
          onClick={() => void handleReconnect()}
          disabled={isBusy}
        >
          Reconnect Microsoft
        </button>
      </div>

      {error && (
        <p className="upload-error" role="alert">
          {error}
        </p>
      )}

      <App />
    </>
  )

  async function handleReconnect() {
  setError(null)

  try {
    const account =
      instance.getActiveAccount() ?? accounts[0]

    if (!account) {
      await instance.loginRedirect({
        scopes: invoiceApiScopes,
      })
      return
    }

    await instance.acquireTokenRedirect({
      account,
      scopes: invoiceApiScopes,
    })
  } catch (err) {
    setError(
      err instanceof Error
        ? err.message
        : 'Could not reconnect your Microsoft session.'
    )
  }
}
}