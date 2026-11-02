import { useMockHandlers, resetMockHandlers } from '../../../../mocks/index.js';
import { UmbSchemaLockdownContext } from './schema-lockdown.context.js';
import { aTimeout, expect, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type { ServerSchemaLockdownResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

const { http, HttpResponse } = window.MockServiceWorker;

@customElement('umb-test-schema-lockdown-host')
export class UmbTestSchemaLockdownHostElement extends UmbElementMixin(HTMLElement) {}

describe('UmbSchemaLockdownContext', () => {
	let host: UmbTestSchemaLockdownHostElement;
	let requested: boolean;

	const respondWith = (response: ServerSchemaLockdownResponseModel | number) =>
		useMockHandlers(
			http.get(umbracoPath('/server/schema-lockdown'), () => {
				requested = true;
				return typeof response === 'number'
					? new HttpResponse(null, { status: response })
					: HttpResponse.json<ServerSchemaLockdownResponseModel>(response);
			}),
		);

	const createLoadedContext = async () => {
		const context = new UmbSchemaLockdownContext(host);
		context.observe(context.state, () => {}, null);
		await waitUntil(() => requested, 'the restrictions were not requested');
		await aTimeout(50);
		return context;
	};

	beforeEach(() => {
		requested = false;
		host = new UmbTestSchemaLockdownHostElement();
		document.body.appendChild(host);
	});

	afterEach(() => {
		document.body.innerHTML = '';
		resetMockHandlers();
	});

	it('denies the operations the server reports as not permitted', async () => {
		respondWith({
			restrictedEntityTypes: [{ entityType: 'document-type', create: false, update: false, delete: true }],
		});
		const context = await createLoadedContext();

		expect(context.isAllowed('document-type', 'create')).to.be.false;
		expect(context.isAllowed('document-type', 'update')).to.be.false;
		expect(context.isAllowed('document-type', 'delete')).to.be.true;
	});

	it('applies an entity type to its folders', async () => {
		respondWith({
			restrictedEntityTypes: [{ entityType: 'document-type', create: false, update: true, delete: true }],
		});
		const context = await createLoadedContext();

		expect(context.isAllowed('document-type-folder', 'create')).to.be.false;
	});

	it('matches backoffice entity types named differently on the server', async () => {
		respondWith({
			restrictedEntityTypes: [{ entityType: 'dictionary-item', create: true, update: false, delete: true }],
		});
		const context = await createLoadedContext();

		expect(context.isAllowed('dictionary', 'update')).to.be.false;
	});

	it('permits the operations of entity types the server does not report', async () => {
		respondWith({
			restrictedEntityTypes: [{ entityType: 'document-type', create: false, update: false, delete: false }],
		});
		const context = await createLoadedContext();

		expect(context.isAllowed('data-type', 'update')).to.be.true;
		expect(context.isAllowed('an-entity-type-schema-lockdown-does-not-cover', 'delete')).to.be.true;
	});

	it('permits every operation while the restrictions are loading', () => {
		respondWith({
			restrictedEntityTypes: [{ entityType: 'document-type', create: false, update: false, delete: false }],
		});
		const context = new UmbSchemaLockdownContext(host);
		context.observe(context.state, () => {}, null);

		expect(context.isAllowed('document-type', 'update')).to.be.true;
	});

	it('permits every operation when the restrictions cannot be loaded', async () => {
		respondWith(500);
		const context = await createLoadedContext();

		expect(context.isAllowed('document-type', 'update')).to.be.true;
	});

	it('requests the restrictions once the user is authorized', async () => {
		respondWith({ restrictedEntityTypes: [] });
		const isAuthorized = new UmbBooleanState<boolean>(false);
		new UmbContextProviderController(host, UMB_AUTH_CONTEXT, {
			isAuthorized: isAuthorized.asObservable(),
			getHostElement: () => host,
		} as never);
		new UmbSchemaLockdownContext(host);
		await aTimeout(50);
		expect(requested, 'the restrictions were requested before the user was authorized').to.be.false;

		isAuthorized.setValue(true);

		await waitUntil(() => requested, 'the restrictions were not requested');
	});

	it('requests the restrictions again for the next observer after a failed request', async () => {
		respondWith(500);
		const context = await createLoadedContext();

		respondWith({
			restrictedEntityTypes: [{ entityType: 'document-type', create: true, update: false, delete: true }],
		});

		// The failed response is handled some time after the request, so keep adding observers until one requests again.
		await waitUntil(() => {
			context.observe(context.state, () => {}, null);
			return context.isAllowed('document-type', 'update') === false;
		}, 'the restrictions were not requested again');
	});
});
