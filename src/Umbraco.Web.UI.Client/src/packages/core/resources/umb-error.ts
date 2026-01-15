import type { UmbProblemDetails } from './types.js';

export class UmbError extends Error {
	public override name = 'UmbError';

	public static isUmbError(error: unknown): error is UmbError {
		return error instanceof UmbError || (error as UmbError).name === 'UmbError';
	}
}

export class UmbCancelError extends UmbError {
	public override name = 'UmbCancelError';

	public static isUmbCancelError(error: unknown): error is UmbCancelError {
		return error instanceof UmbCancelError || (error as UmbCancelError).name === 'UmbCancelError';
	}

	/**
	 * Transforms a CancelError into an UmbCancelError.
	 * @param {*} error The CancelError to transform.
	 * @returns {UmbCancelError} The transformed UmbCancelError.
	 * @deprecated Use `UmbCancelError.isUmbCancelError` instead and map your object to `UmbCancelError` if needed.
	 */
	public static fromLegacyCancelError(error: Error): UmbCancelError {
		return new UmbCancelError(error.message);
	}
}

/**
 * UmbApiError is a class that extends UmbError and represents an error that occurs during an API call.
 */
export class UmbApiError extends UmbError {
	public override name = 'UmbApiError';
	public status: number;
	public request: unknown;
	public problemDetails: UmbProblemDetails;

	public constructor(message: string, status: number, request: unknown, problemDetails: UmbProblemDetails) {
		super(message);
		this.status = status;
		this.request = request;
		this.problemDetails = problemDetails;
	}

	public static isUmbApiError(error: unknown): error is UmbApiError {
		return error instanceof UmbApiError || (error as UmbApiError).name === 'UmbApiError';
	}

	/**
	 * Transforms an ApiError into an UmbApiError.
	 * @param {*} error The ApiError to transform.
	 * @returns {UmbApiError} The transformed UmbApiError.
	 * @deprecated Use `UmbCancelError.isUmbApiError` instead and map your object to `UmbApiError` if needed.
	 */
	public static fromLegacyApiError(error: Error & { body?: string; status?: number; request?: unknown }): UmbApiError {
		// ApiError - body could hold a ProblemDetails from the server
		let problemDetails: UmbProblemDetails | null = null;
		if (typeof error.body !== 'undefined' && !!error.body) {
			try {
				problemDetails = typeof error.body === 'string' ? JSON.parse(error.body) : error.body;
			} catch (e) {
				console.error('Error parsing error body (expected JSON)', e);
			}
		}
		return new UmbApiError(
			error.message,
			error.status ?? 0,
			error.request,
			problemDetails ?? { title: error.message, type: 'ApiError', status: error.status ?? 0 },
		);
	}
}
