import { UmbEntityDetailWorkspaceContextBase } from '../entity-detail-workspace-base.js';
import { UmbEntityDetailWorkspaceEditorElement } from './entity-detail-workspace-editor.element.js';
import type { UmbEntityDetailForbiddenElement } from './entity-detail-forbidden.element.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

@customElement('umb-test-entity-detail-workspace-editor-host')
class UmbTestEntityDetailWorkspaceEditorHostElement extends UmbElementMixin(HTMLElement) {}

class UmbTestEntityDetailWorkspaceContext extends UmbEntityDetailWorkspaceContextBase {
	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: 'Umb.Test.Workspace.EntityDetailEditor',
			entityType: 'umb-test-entity-detail-editor',
			detailRepositoryAlias: 'Umb.Test.Repository.EntityDetailEditor',
		});
	}
}

describe('UmbEntityDetailWorkspaceEditorElement', () => {
	let context: UmbTestEntityDetailWorkspaceContext;
	let element: UmbEntityDetailWorkspaceEditorElement;

	beforeEach(async () => {
		const host = new UmbTestEntityDetailWorkspaceEditorHostElement();
		document.body.appendChild(host);
		context = new UmbTestEntityDetailWorkspaceContext(host);
		element = new UmbEntityDetailWorkspaceEditorElement();
		host.appendChild(element);
		await element.updateComplete;
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	async function forbiddenElement() {
		await element.updateComplete;
		return element.shadowRoot?.querySelector<UmbEntityDetailForbiddenElement>('umb-entity-detail-forbidden') ?? null;
	}

	it('does not show the forbidden state while nothing is forbidden', async () => {
		expect(await forbiddenElement()).to.be.null;
	});

	it('shows the forbidden state with its reason', async () => {
		context.forbidden.addState({ unique: 'test', message: 'Locked by a rule', reason: { title: 'Locked', detail: 'Unlock it first' } });

		expect((await forbiddenElement())?.reason).to.deep.equal({ title: 'Locked', detail: 'Unlock it first' });
	});

	it('shows the first forbidden state', async () => {
		context.forbidden.addState({ unique: 'first', reason: { title: 'Locked' } });
		context.forbidden.addState({ unique: 'second', reason: { title: 'Archived' } });

		expect((await forbiddenElement())?.reason?.title).to.equal('Locked');
	});

	it('shows the default forbidden text for a state without a reason', async () => {
		context.forbidden.addState({ unique: 'test', message: 'Forbidden' });

		const forbidden = await forbiddenElement();
		expect(forbidden).to.not.be.null;
		expect(forbidden?.reason).to.be.undefined;
	});

	it('hides the workspace editor while forbidden', async () => {
		context.setData({ entityType: 'umb-test-entity-detail-editor', unique: '1' });
		await element.updateComplete;
		const workspaceEditor = element.shadowRoot?.querySelector('umb-workspace-editor');
		expect(workspaceEditor?.classList.contains('hide')).to.be.false;

		context.forbidden.addState({ unique: 'test' });
		await element.updateComplete;

		expect(workspaceEditor?.classList.contains('hide')).to.be.true;
	});

	it('stops showing the forbidden state once it is removed', async () => {
		context.forbidden.addState({ unique: 'test', reason: { title: 'Locked' } });
		await element.updateComplete;
		context.forbidden.removeState('test');

		expect(await forbiddenElement()).to.be.null;
	});
});
