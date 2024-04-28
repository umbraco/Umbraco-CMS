import type { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface IUmbIsTrashedContext extends UmbContextBase<IUmbIsTrashedContext> {
	isTrashed: Observable<boolean>;
	getIsTrashed(): boolean;
	setIsTrashed(isTrashed: boolean): void;
}
