import { UmbDocumentDynamicRootResolver } from './document-dynamic-root-resolver.controller.js';
import { UMB_CONTENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/content';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbDynamicRootRepository } from '@umbraco-cms/backoffice/dynamic-root';
import { UmbParentEntityContext } from '@umbraco-cms/backoffice/entity';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { expect } from '@open-wc/testing';
import type { UmbDynamicRoot } from '@umbraco-cms/backoffice/dynamic-root';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

@customElement('umb-test-document-dynamic-root-resolver-host')
class UmbTestDocumentDynamicRootResolverHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

type RequestRootArguments = Parameters<UmbDynamicRootRepository['requestRoot']>;

interface UmbTestResolverOptions {
	/** The content workspace the resolver runs in. `isNew` is left out for a workspace that cannot tell. */
	workspace?: { unique: string; isNew?: boolean };
	parent?: UmbEntityModel;
}

describe('UmbDocumentDynamicRootResolver', () => {
	const dynamicRoot: UmbDynamicRoot = { originAlias: 'Current', querySteps: [] };
	const originalRequestRoot = UmbDynamicRootRepository.prototype.requestRoot;

	let hosts: Array<UmbTestDocumentDynamicRootResolverHostElement>;
	let requestRootCalls: Array<RequestRootArguments>;
	let requestRootResult: Array<string> | undefined;

	function createResolver(options: UmbTestResolverOptions = {}) {
		const host = new UmbTestDocumentDynamicRootResolverHostElement();
		document.body.appendChild(host);
		hosts.push(host);

		const workspace = options.workspace ?? { unique: 'document-key', isNew: false };
		const workspaceContext: Record<string, unknown> = {
			IS_CONTENT_WORKSPACE_CONTEXT: true,
			getHostElement: () => host,
			getUnique: () => workspace.unique,
		};
		if (workspace.isNew !== undefined) {
			workspaceContext.getIsNew = () => workspace.isNew;
		}
		new UmbContextProviderController(host, UMB_CONTENT_WORKSPACE_CONTEXT, workspaceContext as never);

		new UmbParentEntityContext(host).setParent(options.parent ?? { unique: 'parent-key', entityType: 'document' });

		return new UmbDocumentDynamicRootResolver(host);
	}

	beforeEach(() => {
		hosts = [];
		requestRootCalls = [];
		requestRootResult = ['resolved-root'];
		UmbDynamicRootRepository.prototype.requestRoot = async function (...args: RequestRootArguments) {
			requestRootCalls.push(args);
			return requestRootResult;
		};
	});

	afterEach(() => {
		UmbDynamicRootRepository.prototype.requestRoot = originalRequestRoot;
		hosts.forEach((host) => host.remove());
	});

	describe('resolveStartNodeUnique', () => {
		it('resolves nothing, without asking the server, when there is no dynamic root', async () => {
			const result = await createResolver().resolveStartNodeUnique(undefined);

			expect(result).to.equal(undefined);
			expect(requestRootCalls).to.have.length(0);
		});

		it('asks the server to resolve the dynamic root against the document being edited and its parent', async () => {
			await createResolver().resolveStartNodeUnique(dynamicRoot);

			expect(requestRootCalls).to.deep.equal([[dynamicRoot, 'document-key', 'parent-key']]);
		});

		it('does not send the key of a document that has not been saved yet', async () => {
			const resolver = createResolver({ workspace: { unique: 'client-generated-key', isNew: true } });

			await resolver.resolveStartNodeUnique(dynamicRoot);

			expect(requestRootCalls).to.deep.equal([[dynamicRoot, null, 'parent-key']]);
		});

		it('sends the key of a workspace that cannot tell whether its document is new', async () => {
			const resolver = createResolver({ workspace: { unique: 'document-key' } });

			await resolver.resolveStartNodeUnique(dynamicRoot);

			expect(requestRootCalls).to.deep.equal([[dynamicRoot, 'document-key', 'parent-key']]);
		});

		it('sends no parent key when the parent is the root of the tree', async () => {
			const resolver = createResolver({ parent: { unique: null, entityType: 'document-root' } });

			await resolver.resolveStartNodeUnique(dynamicRoot);

			expect(requestRootCalls).to.deep.equal([[dynamicRoot, 'document-key', null]]);
		});

		it('resolves to the first root the server returns', async () => {
			requestRootResult = ['first-root', 'second-root'];

			const result = await createResolver().resolveStartNodeUnique(dynamicRoot);

			expect(result).to.equal('first-root');
		});

		it('resolves nothing when the server returns no roots', async () => {
			requestRootResult = [];

			const result = await createResolver().resolveStartNodeUnique(dynamicRoot);

			expect(result).to.equal(undefined);
		});

		it('resolves nothing when the server returns no response', async () => {
			requestRootResult = undefined;

			const result = await createResolver().resolveStartNodeUnique(dynamicRoot);

			expect(result).to.equal(undefined);
		});
	});
});
