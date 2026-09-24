import { UmbPropertyActionMenuElement } from './property-action-menu.element.js';
import type { ManifestPropertyAction } from '../../property-action.extension.js';
import { UmbPropertyActionBase } from '../../property-action-base.js';
import { expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('umb-test-property-action-menu-action')
class UmbTestPropertyActionElement extends UmbControllerHostElementMixin(HTMLElement) {}

class UmbTestPropertyActionApi extends UmbPropertyActionBase {
	override async execute() {}
}

const PROPERTY_EDITOR_UI_ALIAS = 'Umb.Test.PropertyActionMenu';

function sleep(timeMs: number) {
	return new Promise((resolve) => setTimeout(resolve, timeMs));
}

describe('UmbPropertyActionMenuElement', () => {
	let aliases: Array<string> = [];

	async function renderSequence(actions: Array<{ alias: string; weight: number; group?: string }>) {
		aliases = actions.map((a) => a.alias);
		umbExtensionsRegistry.registerMany(
			actions.map(
				(action): ManifestPropertyAction => ({
					type: 'propertyAction',
					alias: action.alias,
					name: action.alias,
					weight: action.weight,
					group: action.group,
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
		await sleep(100);
		const layout = element.shadowRoot!.querySelector('umb-popover-layout')!;
		return Array.from(layout.children).map((child) =>
			child.getAttribute('role') === 'separator'
				? '|'
				: child instanceof UmbTestPropertyActionElement
					? (child as any).manifest.alias
					: child.tagName,
		);
	}

	afterEach(() => {
		umbExtensionsRegistry.unregisterMany(aliases);
	});

	it('renders no separators when no action has a group', async () => {
		expect(
			await renderSequence([
				{ alias: 'a', weight: 2 },
				{ alias: 'b', weight: 1 },
			]),
		).to.deep.equal(['a', 'b']);
	});

	it('renders a separator between adjacent actions of different groups', async () => {
		expect(
			await renderSequence([
				{ alias: 'a', weight: 3, group: 'clipboard' },
				{ alias: 'b', weight: 2, group: 'clipboard' },
				{ alias: 'c', weight: 1 },
			]),
		).to.deep.equal(['a', 'b', '|', 'c']);
	});
});
