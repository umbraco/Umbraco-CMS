import { UmbEntityDetailWorkspaceContextBase } from './entity-detail-workspace-base.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

interface UmbTestDetailModel extends UmbEntityModel {
	name: string;
}

@customElement('umb-test-entity-detail-workspace-host')
class UmbTestEntityDetailWorkspaceHostElement extends UmbElementMixin(HTMLElement) {}

class UmbTestEntityDetailWorkspaceContext extends UmbEntityDetailWorkspaceContextBase<UmbTestDetailModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: 'Umb.Test.Workspace.EntityDetail',
			entityType: 'umb-test-entity-detail',
			detailRepositoryAlias: 'Umb.Test.Repository.EntityDetail',
		});
	}

	protected override _checkWillNavigateAway(): boolean {
		return true;
	}

	setUnpersistedChanges() {
		this._data.setPersisted({ entityType: 'umb-test-entity-detail', unique: 'test', name: '' });
		this._data.setCurrent({ entityType: 'umb-test-entity-detail', unique: 'test', name: 'Changed' });
	}
}

describe('UmbEntityDetailWorkspaceContextBase', () => {
	let context: UmbTestEntityDetailWorkspaceContext;

	beforeEach(() => {
		const host = new UmbTestEntityDetailWorkspaceHostElement();
		document.body.appendChild(host);
		context = new UmbTestEntityDetailWorkspaceContext(host);
		context.setUnpersistedChanges();
	});

	afterEach(() => {
		context.destroy();
		document.body.innerHTML = '';
	});

	const dispatchWillChangeState = () => {
		const event = new CustomEvent('willchangestate', { cancelable: true, detail: { url: '/elsewhere' } });
		window.dispatchEvent(event);
		return event;
	};

	const dispatchBeforeUnload = () => {
		const event = new Event('beforeunload', { cancelable: true });
		window.dispatchEvent(event);
		return event;
	};

	it('holds navigation while there are unpersisted changes', () => {
		expect(dispatchWillChangeState().defaultPrevented).to.be.true;
	});

	it('does not hold navigation while the workspace is forbidden', () => {
		context.forbidden.addState({ unique: 'test' });

		expect(dispatchWillChangeState().defaultPrevented).to.be.false;
	});

	it('warns before unload while there are unpersisted changes', () => {
		expect(dispatchBeforeUnload().defaultPrevented).to.be.true;
	});

	it('does not warn before unload while the workspace is forbidden', () => {
		context.forbidden.addState({ unique: 'test' });

		expect(dispatchBeforeUnload().defaultPrevented).to.be.false;
	});
});
