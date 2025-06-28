import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbGuardManagerBase, type UmbGuardRule } from '@umbraco-cms/backoffice/utils';

export class UmbNameWriteGuardManager extends UmbGuardManagerBase {
	public isPermittedForName(): Observable<boolean> {
		return this._rules.asObservablePart((rules) => this.#resolvePermission(rules));
	}

	#resolvePermission(rules: Array<UmbGuardRule>): boolean {
		if (rules.some((rule) => rule.permitted === false)) {
			return false;
		}

		if (rules.some((rule) => rule.permitted === true)) {
			return true;
		}

		return this._fallback;
	}
}
