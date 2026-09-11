import { UmbApiError, UmbCancelError } from '../umb-error.js';

/**
 * Codes that are ignored for notifications.
 * These are typically non-fatal errors that the UI can handle gracefully,
 * such as 401 (Unauthorized), 403 (Forbidden), and 404 (Not Found).
 * The UI should handle these cases without showing a notification.
 */
const IGNORED_ERROR_CODES = new Set([401, 403, 404]);

/**
 * Operation statuses that are ignored for notifications.
 * These are operation statuses where the server already sends a notification
 * via the Umb-Notifications response header, so we avoid showing a duplicate
 * notification from the ProblemDetails body.
 */
const IGNORED_OPERATION_STATUSES = new Set(['CancelledByNotification']);

/**
 * Whether `tryExecute` surfaces a notification for this error.
 *
 * This is the single definition of that rule: `tryExecute` uses it to decide whether to notify, and callers
 * that add their own fallback messaging use it to avoid duplicating or contradicting the notification the
 * user has already been shown.
 * @param {unknown} error The error to inspect, typically the `error` of an `UmbApiResponse` or the `cause` of a wrapping error.
 * @returns {boolean} True when a notification is shown for this error, false when it is suppressed.
 */
export function apiErrorWasNotified(error: unknown): boolean {
	// The UmbError type guards read `.name` off the argument, so anything that is not an object has to be
	// rejected first. It also cannot have come from tryExecute, so nothing was notified for it.
	if (error === null || typeof error !== 'object') {
		return false;
	}

	if (UmbCancelError.isUmbCancelError(error)) {
		return false;
	}

	if (!UmbApiError.isUmbApiError(error) || !error.problemDetails) {
		// Falls through to the generic "unknown error" notification.
		return true;
	}

	if (IGNORED_ERROR_CODES.has(error.problemDetails.status)) {
		return false;
	}

	return !(
		error.problemDetails.operationStatus && IGNORED_OPERATION_STATUSES.has(error.problemDetails.operationStatus)
	);
}
