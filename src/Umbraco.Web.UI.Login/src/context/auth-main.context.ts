import { IUmbAuthContext } from '../types.js';
import { UmbAuthLegacyContext } from './auth-legacy.context.js';

export class UmbAuthMainContext {
	private static instance: IUmbAuthContext;
	public static get Instance() {
		if (!UmbAuthMainContext.instance) {
			this.instance = new UmbAuthLegacyContext();
		}
		return UmbAuthMainContext.instance;
	}
}
