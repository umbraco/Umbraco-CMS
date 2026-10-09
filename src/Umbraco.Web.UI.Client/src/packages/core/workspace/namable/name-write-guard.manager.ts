import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { mergeObservables } from '@umbraco-cms/backoffice/observable-api';
import { UmbGuardManagerBase, type UmbGuardRule } from '@umbraco-cms/backoffice/utils';

export class UmbNameWriteGuardManager extends UmbGuardManagerBase {
	public isPermittedForName(): Observable<boolean> {
		return mergeObservables([this.rules, this._fallback], ([rules, fallback]) => {
			return this.#resolvePermission(rules) ?? fallback;
		});
	}

	#resolvePermission(rules: Array<UmbGuardRule>): boolean | undefined {
		if (rules.some((rule) => rule.permitted === false)) {
			return false;
		}

		if (rules.some((rule) => rule.permitted === true)) {
			return true;
		}

		return undefined;
	}
}
