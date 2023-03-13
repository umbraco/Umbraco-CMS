import { UmbContextProviderController, UmbContextToken } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbModalToken } from '@umbraco-cms/modal';

type GetResultType<T> = T extends UmbModalToken<infer Data, infer Result> ? Result : unknown;

export type UmbModalRoute<UmbModalTokenResult> = {
	path: string;
	onSetup: (routeInfo: any) => void;
	onSubmit: (data: UmbModalTokenResult) => void;
	onReject: () => void;
};

export class UmbRouteContext {
	#host: UmbControllerHostInterface;

	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
		new UmbContextProviderController(host, UMB_ROUTE_CONTEXT_TOKEN, this);
	}

	public registerModal<T extends UmbModalToken = UmbModalToken, R = GetResultType<T>>(
		modalAlias: T,
		options: UmbModalRoute<R>
	) {
		console.log('registerModalRoutee', modalAlias.toString(), options);
	}
}

export const UMB_ROUTE_CONTEXT_TOKEN = new UmbContextToken<UmbRouteContext>('UmbSectionContext');
