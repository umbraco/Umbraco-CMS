import { UmbEntityNamedDetailWorkspaceContextBase } from './entity-named-detail-workspace-base.js';
import { expect, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

@customElement('umb-test-entity-named-detail-workspace-host')
class UmbTestEntityNamedDetailWorkspaceHostElement extends UmbElementMixin(HTMLElement) {}

class UmbTestEntityNamedDetailWorkspaceContext extends UmbEntityNamedDetailWorkspaceContextBase {
	submitted = false;

	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: 'Umb.Test.Workspace.EntityNamedDetail',
			entityType: 'umb-test-entity-named-detail',
			detailRepositoryAlias: 'Umb.Test.Repository.EntityNamedDetail',
		});
	}

	override async submit() {
		this.submitted = true;
	}
}

describe('UmbEntityNamedDetailWorkspaceContextBase', () => {
	let context: UmbTestEntityNamedDetailWorkspaceContext;

	beforeEach(() => {
		const host = new UmbTestEntityNamedDetailWorkspaceHostElement();
		document.body.appendChild(host);
		context = new UmbTestEntityNamedDetailWorkspaceContext(host);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	const isNamePermitted = () => context.nameWriteGuard.getRules().every((rule) => rule.permitted !== false);

	it('prevents renaming while the entity is read-only', async () => {
		context.readOnlyGuard.addRule({ unique: 'test', permitted: true });

		await waitUntil(() => !isNamePermitted(), 'renaming was not prevented');

		context.readOnlyGuard.removeRule('test');

		await waitUntil(isNamePermitted, 'renaming was not allowed again');
	});

	it('is read-only only through rules that target the whole entity', () => {
		context.readOnlyGuard.addRule({ unique: 'variant', permitted: true, variantId: {} } as never);
		expect(context.getReadOnly()).to.be.false;

		context.readOnlyGuard.addRule({ unique: 'entity', permitted: true });
		expect(context.getReadOnly()).to.be.true;
	});

	it('only reports read-only changes when the whole entity changes between read-only and editable', () => {
		const emitted: Array<boolean> = [];
		const subscription = context.readOnly.subscribe((isReadOnly) => emitted.push(isReadOnly));

		context.readOnlyGuard.addRule({ unique: 'variant', permitted: true, variantId: {} } as never);
		context.readOnlyGuard.addRule({ unique: 'entity', permitted: true });
		context.readOnlyGuard.addRule({ unique: 'another-entity', permitted: true });
		subscription.unsubscribe();

		expect(emitted).to.deep.equal([false, true]);
	});

	it('does not submit while the entity is read-only', async () => {
		context.readOnlyGuard.addRule({ unique: 'test', permitted: true });

		await context.requestSubmit();

		expect(context.submitted).to.be.false;
	});

	it('submits once the entity is no longer read-only', async () => {
		context.readOnlyGuard.addRule({ unique: 'test', permitted: true });
		context.readOnlyGuard.removeRule('test');

		await context.requestSubmit();

		expect(context.submitted).to.be.true;
	});

	it('clears read-only rules when the workspace state is reset for another entity', () => {
		context.readOnlyGuard.addRule({ unique: 'test', permitted: true });

		context.resetState();

		expect(context.readOnlyGuard.getRules()).to.be.empty;
		expect(context.getReadOnly()).to.be.false;
	});
});
