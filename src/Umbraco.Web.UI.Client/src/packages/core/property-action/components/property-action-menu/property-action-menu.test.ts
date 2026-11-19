import { UmbPropertyActionMenuElement } from './property-action-menu.element.js';
import type { ManifestPropertyAction } from '../../property-action.extension.js';
import { UmbPropertyActionBase } from '../../property-action-base.js';
import { expect, fixture, html, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { readActionGroupList } from '@umbraco-cms/internal/test-utils';

@customElement('umb-test-property-action-menu-action')
class UmbTestPropertyActionElement extends UmbControllerHostElementMixin(HTMLElement) {}

// Stops the click from propagating, as `umb-property-action` does when it executes its action.
@customElement('umb-test-property-action-menu-stopping-action')
class UmbTestStoppingPropertyActionElement extends UmbControllerHostElementMixin(HTMLElement) {
	constructor() {
		super();
		this.addEventListener('click', (event) => event.stopPropagation());
	}
}

class UmbTestPropertyActionApi extends UmbPropertyActionBase {
	override async execute() {}
}

const PROPERTY_EDITOR_UI_ALIAS = 'Umb.Test.PropertyActionMenu';

describe('UmbPropertyActionMenuElement', () => {
	let aliases: Array<string> = [];

	async function renderSequence(
		actions: Array<{ alias: string; weight: number; group?: string; separatorBefore?: boolean }>,
	) {
		aliases = actions.map((a) => a.alias);
		umbExtensionsRegistry.registerMany(
			actions.map(
				(action): ManifestPropertyAction => ({
					type: 'propertyAction',
					alias: action.alias,
					name: action.alias,
					weight: action.weight,
					group: action.group,
					separatorBefore: action.separatorBefore,
					forPropertyEditorUis: [PROPERTY_EDITOR_UI_ALIAS],
					elementName: 'umb-test-property-action-menu-action',
					api: UmbTestPropertyActionApi,
					meta: {},
				}),
			),
		);
		const element = await fixture<UmbPropertyActionMenuElement>(
			html`<umb-property-action-menu .propertyEditorUiAlias=${PROPERTY_EDITOR_UI_ALIAS}></umb-property-action-menu>`,
		);
		const read = () =>
			readActionGroupList(element.shadowRoot?.querySelector('umb-action-group-list'), (child) =>
				child instanceof UmbTestPropertyActionElement ? (child as any).manifest.alias : child.tagName,
			);
		await waitUntil(() => read().filter((entry) => aliases.includes(entry)).length === actions.length);
		return read();
	}

	afterEach(() => {
		umbExtensionsRegistry.unregisterMany(aliases);
	});

	it('renders no separators when no action sets separatorBefore', async () => {
		expect(
			await renderSequence([
				{ alias: 'a', weight: 2 },
				{ alias: 'b', weight: 1 },
			]),
		).to.deep.equal(['[', 'a', 'b', ']']);
	});

	it('starts a new line for each action that sets separatorBefore, except the first', async () => {
		expect(
			await renderSequence([
				{ alias: 'a', weight: 3, separatorBefore: true },
				{ alias: 'b', weight: 2 },
				{ alias: 'c', weight: 1, separatorBefore: true },
			]),
		).to.deep.equal(['[', 'a', 'b', '|', 'c', ']']);
	});

	describe('action groups', () => {
		const GROUPS = ['Umb.Test.PropertyActionMenu.Clipboard', 'Umb.Test.PropertyActionMenu.Edit'];

		beforeEach(() => {
			umbExtensionsRegistry.registerMany([
				{ type: 'propertyActionGroup', alias: GROUPS[0], name: GROUPS[0], weight: 1200, meta: { label: 'Clipboard' } },
				{ type: 'propertyActionGroup', alias: GROUPS[1], name: GROUPS[1], weight: 1100, meta: { label: 'Edit' } },
			]);
		});

		afterEach(() => {
			umbExtensionsRegistry.unregisterMany(GROUPS);
		});

		it('renders each group as its own block and the ungrouped actions last', async () => {
			expect(
				await renderSequence([
					{ alias: 'clear', weight: 0, separatorBefore: true },
					{ alias: 'package', weight: 5000 },
					{ alias: 'sort', weight: 1000, group: GROUPS[1] },
					{ alias: 'paste', weight: 1190, group: GROUPS[0] },
					{ alias: 'copy', weight: 1200, group: GROUPS[0] },
				]),
			).to.deep.equal([
				'[Clipboard',
				'copy',
				'paste',
				']',
				'[Edit',
				'sort',
				']',
				'[Other',
				'package',
				'|',
				'clear',
				']',
			]);
		});
	});

	it('closes the menu once an action is clicked, even when the action stops the click from propagating', async () => {
		aliases = ['stopping-action'];
		umbExtensionsRegistry.register({
			type: 'propertyAction',
			alias: 'stopping-action',
			name: 'stopping-action',
			forPropertyEditorUis: [PROPERTY_EDITOR_UI_ALIAS],
			elementName: 'umb-test-property-action-menu-stopping-action',
			api: UmbTestPropertyActionApi,
			meta: {},
		} as ManifestPropertyAction);
		const element = await fixture<UmbPropertyActionMenuElement>(
			html`<umb-property-action-menu .propertyEditorUiAlias=${PROPERTY_EDITOR_UI_ALIAS}></umb-property-action-menu>`,
		);
		const findAction = () =>
			element.shadowRoot
				?.querySelector('umb-action-group-list')
				?.shadowRoot?.querySelector<UmbTestStoppingPropertyActionElement>(
					'umb-test-property-action-menu-stopping-action',
				);
		await waitUntil(() => !!findAction(), 'action was not rendered');

		const popover = element.shadowRoot!.querySelector<HTMLElement>('#property-action-popover')!;
		element.shadowRoot!.querySelector<HTMLElement>('#popover-trigger')!.click();
		await waitUntil(() => popover.matches(':popover-open'), 'menu did not open');

		findAction()!.click();

		await waitUntil(() => !popover.matches(':popover-open'), 'menu is still open after the action was clicked');
	});
});
