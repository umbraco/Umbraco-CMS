import { UmbTemplateWorkspaceContext } from './templates/workspace/template-workspace.context.js';
import { UmbPartialViewWorkspaceContext } from './partial-views/workspace/partial-view-workspace.context.js';
import { expect, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { UmbBasicState } from '@umbraco-cms/backoffice/observable-api';
import { UMB_SERVER_CONTEXT } from '@umbraco-cms/backoffice/server';
import type { UmbServerContext } from '@umbraco-cms/backoffice/server';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

@customElement('umb-test-templating-workspace-host')
class UmbTestHostElement extends UmbElementMixin(HTMLElement) {}

interface UmbTestWorkspaceContext {
	getReadOnly(): boolean;
	resetState(): void;
}

const workspaces: Array<[string, new (host: UmbControllerHost) => UmbTestWorkspaceContext]> = [
	['UmbTemplateWorkspaceContext', UmbTemplateWorkspaceContext],
	['UmbPartialViewWorkspaceContext', UmbPartialViewWorkspaceContext],
];

workspaces.forEach(([name, WorkspaceContext]) => {
	describe(name, () => {
		let host: UmbTestHostElement;
		let provider: UmbContextProvider | undefined;
		let isProductionMode: UmbBasicState<boolean | undefined>;
		let context: UmbTestWorkspaceContext;

		function provideServerContext() {
			const stub = {
				getHostElement: () => document.body,
				isProductionMode: isProductionMode.asObservable(),
			} as unknown as UmbServerContext;
			provider = new UmbContextProvider(document.body, UMB_SERVER_CONTEXT, stub);
			provider.hostConnected();
		}

		beforeEach(() => {
			isProductionMode = new UmbBasicState<boolean | undefined>(undefined);
			host = new UmbTestHostElement();
			document.body.appendChild(host);
		});

		afterEach(() => {
			provider?.hostDisconnected();
			provider = undefined;
			host.remove();
		});

		it('is read-only until the runtime mode is known', () => {
			context = new WorkspaceContext(host);

			expect(context.getReadOnly()).to.be.true;
		});

		it('is not read-only outside production mode', async () => {
			provideServerContext();
			context = new WorkspaceContext(host);

			isProductionMode.setValue(false);

			await waitUntil(() => context.getReadOnly() === false, 'the workspace stayed read-only');
		});

		it('is read-only in production mode', async () => {
			provideServerContext();
			context = new WorkspaceContext(host);

			isProductionMode.setValue(false);
			await waitUntil(() => context.getReadOnly() === false, 'the workspace stayed read-only');

			isProductionMode.setValue(true);
			await waitUntil(() => context.getReadOnly() === true, 'the workspace did not become read-only');
		});

		it('stays read-only in production mode when the workspace state is reset for another entity', async () => {
			provideServerContext();
			context = new WorkspaceContext(host);
			isProductionMode.setValue(true);
			await waitUntil(() => context.getReadOnly() === true, 'the workspace did not become read-only');

			context.resetState();

			expect(context.getReadOnly()).to.be.true;
		});

		it('stays editable outside production mode when the workspace state is reset for another entity', async () => {
			provideServerContext();
			context = new WorkspaceContext(host);
			isProductionMode.setValue(false);
			await waitUntil(() => context.getReadOnly() === false, 'the workspace stayed read-only');

			context.resetState();

			expect(context.getReadOnly()).to.be.false;
		});
	});
});
