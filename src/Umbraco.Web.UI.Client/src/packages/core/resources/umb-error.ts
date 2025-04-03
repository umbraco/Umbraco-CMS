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
}

/**
 * UmbApiError is a class that extends UmbError and represents an error that occurs during an API call.
 */
export class UmbApiError extends UmbError {
	public override name = 'UmbApiError';
	public status: number;
	public response: unknown;
	public request: unknown;
	public problemDetails: UmbProblemDetails;

	public constructor(
		message: string,
		status: number,
		response: unknown,
		request: unknown,
		problemDetails: UmbProblemDetails,
	) {
		super(message);
		this.status = status;
		this.response = response;
		this.request = request;
		this.problemDetails = problemDetails;
	}

	public static isUmbApiError(error: unknown): error is UmbApiError {
		return error instanceof UmbApiError || (error as UmbApiError).name === 'UmbApiError';
	}
}
