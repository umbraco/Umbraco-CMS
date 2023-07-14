import { IUmbAuthContext } from '../types.js';
import { UmbAuthLegacyContext } from './auth-legacy.context.js';
import { UmbAuthContext } from './auth.context.js';

export class UmbAuthMainContext {
	private static instance: IUmbAuthContext;
	public static get Instance() {
		if (!UmbAuthMainContext.instance) {
			throw new Error('UmbAuthMainContext not initialized');
		}
		return UmbAuthMainContext.instance;
	}

	constructor(isLegacy = false) {
		if (UmbAuthMainContext.instance) {
			throw new Error('UmbAuthMainContext already initialized');
		}

		UmbAuthMainContext.instance = isLegacy ? new UmbAuthLegacyContext() : new UmbAuthContext();
	}
}
