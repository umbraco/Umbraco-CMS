import { UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT } from '../../entity-detail/entity-detail-workspace.context-token.js';
import { UMB_WORKSPACE_ENTITY_IS_READ_ONLY_CONDITION_ALIAS } from './constants.js';
import { UmbWorkspaceEntityIsReadOnlyCondition } from './workspace-entity-is-read-only.condition.js';
import { expect, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { UmbReadOnlyGuardManager, type UmbGuardRule } from '@umbraco-cms/backoffice/utils';

@customElement('test-controller-host-workspace-entity-is-read-only')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

@customElement('test-controller-child-workspace-entity-is-read-only')
class UmbTestControllerChildElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbWorkspaceEntityIsReadOnlyCondition', () => {
	let hostElement: UmbTestControllerHostElement;
	let childElement: UmbTestControllerChildElement;
	let readOnlyGuard: UmbReadOnlyGuardManager<UmbGuardRule>;
	let condition: UmbWorkspaceEntityIsReadOnlyCondition;

	const createCondition = (match?: boolean) =>
		new UmbWorkspaceEntityIsReadOnlyCondition(childElement, {
			host: childElement,
			config: { alias: UMB_WORKSPACE_ENTITY_IS_READ_ONLY_CONDITION_ALIAS, match },
			onChange: () => {},
		});

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		childElement = new UmbTestControllerChildElement();
		hostElement.appendChild(childElement);
		document.body.appendChild(hostElement);

		readOnlyGuard = new UmbReadOnlyGuardManager<UmbGuardRule>(hostElement);
		const workspaceContext = {
			IS_ENTITY_DETAIL_WORKSPACE_CONTEXT: true,
			readOnly: readOnlyGuard.permitted,
			getHostElement: () => hostElement,
		};
		new UmbContextProvider(hostElement, UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT, workspaceContext as never).hostConnected();
	});

	afterEach(() => {
		condition?.hostDisconnected();
		document.body.innerHTML = '';
	});

	it('is permitted while the entity is read-only, by default', async () => {
		readOnlyGuard.addRule({ unique: 'test', permitted: true });
		condition = createCondition();

		await waitUntil(() => condition.permitted, 'the condition was not permitted');
	});

	it('is not permitted while the entity is editable, by default', async () => {
		condition = createCondition();
		await waitUntil(() => readOnlyGuard.getPermitted() === false);

		expect(condition.permitted).to.be.false;
	});

	it('is permitted while the entity is editable when matching false', async () => {
		condition = createCondition(false);

		await waitUntil(() => condition.permitted, 'the condition was not permitted');
	});

	it('follows the entity becoming read-only and editable again', async () => {
		condition = createCondition(false);
		await waitUntil(() => condition.permitted, 'the condition was not permitted');

		readOnlyGuard.addRule({ unique: 'test', permitted: true });
		await waitUntil(() => !condition.permitted, 'the condition stayed permitted while read-only');

		readOnlyGuard.removeRule('test');
		await waitUntil(() => condition.permitted, 'the condition was not permitted once editable again');
	});
});
