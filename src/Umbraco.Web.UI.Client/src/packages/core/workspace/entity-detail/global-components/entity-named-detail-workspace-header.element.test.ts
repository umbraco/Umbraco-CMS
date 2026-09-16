import { UmbEntityNamedDetailWorkspaceHeaderElement } from './entity-named-detail-workspace-header.element.js';
import { UMB_ENTITY_NAMED_DETAIL_WORKSPACE_CONTEXT } from '../entity-named-detail-workspace.context-token.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin, type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UmbBasicState } from '@umbraco-cms/backoffice/observable-api';
import { UmbEntityStateManager } from '@umbraco-cms/backoffice/entity-state';

@customElement('umb-test-entity-named-detail-workspace-host')
class UmbTestEntityNamedDetailWorkspaceHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

/**
 * A minimal, hand-written stand-in for `UmbEntityNamedDetailWorkspaceContextBase` — carries just the
 * marker property the context token's type-guard checks for, plus what the header element actually reads.
 */
class UmbTestEntityNamedDetailWorkspaceContext {
	readonly IS_ENTITY_NAMED_DETAIL_WORKSPACE_CONTEXT = true;

	#host: UmbControllerHost;
	#name = new UmbBasicState<string | undefined>('Test name');
	readonly name = this.#name.asObservable();
	readonly entityState: UmbEntityStateManager;

	constructor(host: UmbControllerHost) {
		this.#host = host;
		this.entityState = new UmbEntityStateManager(host);
	}

	getHostElement() {
		return this.#host.getHostElement();
	}

	getName() {
		return this.#name.getValue();
	}

	setName(name: string) {
		this.#name.setValue(name);
	}
}

describe('UmbEntityNamedDetailWorkspaceHeaderElement', () => {
	let host: UmbTestEntityNamedDetailWorkspaceHostElement;
	let workspaceContext: UmbTestEntityNamedDetailWorkspaceContext;
	let element: UmbEntityNamedDetailWorkspaceHeaderElement;

	beforeEach(async () => {
		host = document.createElement(
			'umb-test-entity-named-detail-workspace-host',
		) as UmbTestEntityNamedDetailWorkspaceHostElement;
		document.body.appendChild(host);

		workspaceContext = new UmbTestEntityNamedDetailWorkspaceContext(host);
		new UmbContextProviderController(host, UMB_ENTITY_NAMED_DETAIL_WORKSPACE_CONTEXT, workspaceContext as never);

		element = document.createElement(
			'umb-entity-named-detail-workspace-header',
		) as UmbEntityNamedDetailWorkspaceHeaderElement;
		host.appendChild(element);
		await element.updateComplete;
	});

	afterEach(() => {
		document.body.removeChild(host);
	});

	function stateTagsElement() {
		return element.shadowRoot!.querySelector('umb-entity-state-tags') as HTMLElement & { states: unknown };
	}

	it('renders no entity-state tags when the registry is empty', () => {
		expect(stateTagsElement().states).to.deep.equal([]);
	});

	it('passes entityState entries through to umb-entity-state-tags', async () => {
		workspaceContext.entityState.addState({ unique: 'a', label: 'Invited', look: 'warning' });
		await element.updateComplete;

		expect(stateTagsElement().states).to.deep.equal([{ unique: 'a', label: 'Invited', look: 'warning' }]);
	});

	it('updates when the entityState registry changes', async () => {
		workspaceContext.entityState.addState({ unique: 'a', label: 'Invited' });
		await element.updateComplete;

		workspaceContext.entityState.removeState('a');
		await element.updateComplete;

		expect(stateTagsElement().states).to.deep.equal([]);
	});
});
