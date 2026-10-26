import { UMB_SUBMITTABLE_WORKSPACE_CONTEXT } from '../../../../contexts/tokens/index.js';
import { UmbSubmitWorkspaceAction } from './submit.action.js';
import { aTimeout, expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbReadOnlyGuardManager, type UmbGuardRule } from '@umbraco-cms/backoffice/utils';

@customElement('umb-test-submit-workspace-action-host')
class UmbTestSubmitWorkspaceActionHostElement extends UmbElementMixin(HTMLElement) {}

describe('UmbSubmitWorkspaceAction', () => {
	let host: UmbTestSubmitWorkspaceActionHostElement;
	let unique: UmbStringState<undefined>;
	let entityReadOnlyGuard: UmbReadOnlyGuardManager<UmbGuardRule>;
	let submitted: boolean;

	const createAction = () => new UmbSubmitWorkspaceAction(host, { manifest: {} as never, meta: {} } as never);

	const isDisabled = async (action: UmbSubmitWorkspaceAction) =>
		(await action.observe(action.isDisabled).asPromise()) as boolean;

	beforeEach(() => {
		host = new UmbTestSubmitWorkspaceActionHostElement();
		document.body.appendChild(host);

		unique = new UmbStringState(undefined);
		entityReadOnlyGuard = new UmbReadOnlyGuardManager<UmbGuardRule>(host);
		submitted = false;

		const workspaceContext = {
			IS_ENTITY_DETAIL_WORKSPACE_CONTEXT: true,
			unique: unique.asObservable(),
			readOnly: entityReadOnlyGuard.permitted,
			requestSubmit: () => {
				submitted = true;
				return Promise.resolve();
			},
			getHostElement: () => host,
		};
		new UmbContextProviderController(host, UMB_SUBMITTABLE_WORKSPACE_CONTEXT, workspaceContext as never);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	it('is disabled until the workspace entity has loaded', async () => {
		const action = createAction();
		await aTimeout(0);

		expect(await isDisabled(action)).to.be.true;

		unique.setValue('existing');

		expect(await isDisabled(action)).to.be.false;
	});

	it('stays disabled after the workspace entity loads while the entity is read-only', async () => {
		entityReadOnlyGuard.addRule({ unique: 'test', permitted: true });
		const action = createAction();
		await aTimeout(0);

		unique.setValue('existing');

		expect(await isDisabled(action)).to.be.true;
	});

	it('follows the read-only state once the workspace entity has loaded', async () => {
		const action = createAction();
		await aTimeout(0);
		unique.setValue('existing');

		entityReadOnlyGuard.addRule({ unique: 'test', permitted: true });
		expect(await isDisabled(action)).to.be.true;

		entityReadOnlyGuard.removeRule('test');
		expect(await isDisabled(action)).to.be.false;
	});

	it('requests the workspace to submit when executed', async () => {
		const action = createAction();
		await aTimeout(0);

		await action.execute();

		expect(submitted).to.be.true;
	});
});
