import { UmbFacetFilterManager } from '../../facet-filter.manager.js';
import type { ManifestFacetFilter } from '../../facet-filter.extension.js';
import type { UmbFacetFilterValueResolver } from '../../value-resolver/facet-filter-value-resolver.interface.js';
import type { ManifestFacetFilterActiveValue } from '../facet-filter-active-value.extension.js';
import { UmbFacetFilterActiveValueElementBase } from '../base/facet-filter-active-value-element.base.js';
import { manifests as defaultKindManifests } from '../default/manifests.js';
import type { UmbFacetFilterActiveValueExtensionElement } from './facet-filter-active-value-extension.element.js';
import type { UmbFacetFilterActiveValueLayoutElement } from './facet-filter-active-value-layout.element.js';
import './facet-filter-active-value-extension.element.js';
import './facet-filter-active-value-layout.element.js';
import { expect, waitUntil } from '@open-wc/testing';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { umbLocalizationManager, type UmbLocalizationSet } from '@umbraco-cms/backoffice/localization-api';

@customElement('test-facet-filter-active-value-host')
class TestHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

@customElement('test-custom-facet-filter-active-value')
class TestCustomFacetFilterActiveValueElement extends UmbFacetFilterActiveValueElementBase {
	override render() {
		return html`<umb-facet-filter-active-value-layout
			.label=${`custom: ${String(this._resolvedValue)}`}
			@remove=${this._remove}></umb-facet-filter-active-value-layout>`;
	}
}

class TestNameResolver implements UmbFacetFilterValueResolver {
	async resolveValues(values: ReadonlyArray<unknown>) {
		return { data: values.map((v) => ({ entityType: 'test', unique: String(v), name: `Name of ${v}` })) };
	}
	destroy() {}
}

const FILTER_ALIAS = 'Umb.Test.FacetFilter.ActiveValue';

function findValueElement(extension: UmbFacetFilterActiveValueExtensionElement): HTMLElement | undefined {
	const slot = extension.shadowRoot?.querySelector('umb-extension-with-api-slot');
	return (slot?.shadowRoot?.firstElementChild ?? undefined) as HTMLElement | undefined;
}

function findLayout(
	extension: UmbFacetFilterActiveValueExtensionElement,
): UmbFacetFilterActiveValueLayoutElement | undefined {
	return (findValueElement(extension)?.shadowRoot?.querySelector('umb-facet-filter-active-value-layout') ??
		undefined) as UmbFacetFilterActiveValueLayoutElement | undefined;
}

function textOf(extension: UmbFacetFilterActiveValueExtensionElement): string {
	return findLayout(extension)?.shadowRoot?.querySelector('#label')?.textContent?.trim() ?? '';
}

describe('UmbFacetFilterActiveValueExtensionElement', () => {
	let host: TestHostElement;
	let manager: UmbFacetFilterManager;
	let extension: UmbFacetFilterActiveValueExtensionElement;

	beforeEach(() => {
		umbExtensionsRegistry.registerMany(defaultKindManifests);
		umbExtensionsRegistry.register({
			type: 'facetFilter',
			alias: FILTER_ALIAS,
			name: FILTER_ALIAS,
			meta: { label: 'Colour' },
		} as unknown as ManifestFacetFilter);

		host = document.createElement('test-facet-filter-active-value-host') as TestHostElement;
		document.body.appendChild(host);
		manager = new UmbFacetFilterManager(host);
		manager.setFilter({ alias: FILTER_ALIAS, unique: 'red', value: 'red' });

		extension = document.createElement('umb-facet-filter-active-value-extension');
		extension.alias = FILTER_ALIAS;
		extension.unique = 'red';
		extension.value = 'red';
	});

	afterEach(() => {
		umbLocalizationManager.localizations.clear();
		manager.destroy();
		document.body.innerHTML = '';
		for (const type of ['facetFilter', 'facetFilterActiveValue']) {
			for (const ext of umbExtensionsRegistry.getByType(type)) {
				if (ext.alias.startsWith('Umb.Test.')) umbExtensionsRegistry.unregister(ext.alias);
			}
		}
		for (const kind of defaultKindManifests) {
			umbExtensionsRegistry.unregister(kind.alias);
		}
	});

	it('shows the raw value with the default element when no facet filter active value is registered for the filter', async () => {
		host.appendChild(extension);

		await waitUntil(() => textOf(extension) === 'red', 'raw value not rendered');
		expect(findValueElement(extension)?.tagName.toLowerCase()).to.equal('umb-default-facet-filter-active-value');
	});

	it('labels the remove button with the displayed value', async () => {
		const english: UmbLocalizationSet = {
			$code: 'en',
			$dir: 'ltr',
			facetFilter_removeValueLabel: (value: string) => `Remove ${value} filter`,
		};
		umbLocalizationManager.registerLocalization(english);
		umbLocalizationManager.documentLanguage = 'en';
		host.appendChild(extension);

		await waitUntil(
			() =>
				findLayout(extension)?.shadowRoot?.querySelector('uui-button')?.getAttribute('label') === 'Remove red filter',
			'remove button not labelled with the value',
		);
	});

	it('shows the name of the resolved item when the default kind is registered with a valueResolver', async () => {
		umbExtensionsRegistry.register({
			type: 'facetFilterActiveValue',
			kind: 'default',
			alias: 'Umb.Test.FacetFilterActiveValue.Named',
			name: 'Named',
			forFacetFilters: [FILTER_ALIAS],
			valueResolver: TestNameResolver,
		} as unknown as ManifestFacetFilterActiveValue);
		host.appendChild(extension);

		await waitUntil(() => textOf(extension) === 'Name of red', 'resolved name not rendered');
	});

	it('renders the element of the facet filter active value registered for the filter', async () => {
		umbExtensionsRegistry.register({
			type: 'facetFilterActiveValue',
			kind: 'default',
			alias: 'Umb.Test.FacetFilterActiveValue.Custom',
			name: 'Custom',
			forFacetFilters: [FILTER_ALIAS],
			element: TestCustomFacetFilterActiveValueElement,
		} as unknown as ManifestFacetFilterActiveValue);
		host.appendChild(extension);

		await waitUntil(() => textOf(extension) === 'custom: red', 'custom element not rendered');
	});

	it('does not render a facet filter active value registered for another filter', async () => {
		umbExtensionsRegistry.register({
			type: 'facetFilterActiveValue',
			kind: 'default',
			alias: 'Umb.Test.FacetFilterActiveValue.Other',
			name: 'Other',
			forFacetFilters: ['Umb.Test.FacetFilter.Other'],
			element: TestCustomFacetFilterActiveValueElement,
		} as unknown as ManifestFacetFilterActiveValue);
		host.appendChild(extension);

		await waitUntil(() => textOf(extension) === 'red', 'default element not rendered');
	});

	it('lets slotted content replace the label, and still labels the remove button with the label', async () => {
		const english: UmbLocalizationSet = {
			$code: 'en',
			$dir: 'ltr',
			facetFilter_removeValueLabel: (value: string) => `Remove ${value} filter`,
		};
		umbLocalizationManager.registerLocalization(english);
		umbLocalizationManager.documentLanguage = 'en';
		const layout = document.createElement('umb-facet-filter-active-value-layout');
		layout.label = 'Red';
		layout.innerHTML = '<span id="swatch"></span>Red';
		host.appendChild(layout);
		await layout.updateComplete;

		const slot = layout.shadowRoot!.querySelector('slot')!;
		expect(slot.assignedNodes()).to.have.length(2);
		expect(layout.shadowRoot!.querySelector('uui-button')!.getAttribute('label')).to.equal('Remove Red filter');
	});

	it('removes the value from the active filters when the layout remove button is clicked', async () => {
		host.appendChild(extension);
		await waitUntil(() => textOf(extension) === 'red', 'raw value not rendered');

		const button = findLayout(extension)?.shadowRoot?.querySelector('uui-button');
		button?.shadowRoot?.querySelector('button')?.click();

		const activeFilters = await new Promise<Array<unknown>>((resolve) => {
			manager.activeFilters.subscribe(resolve).unsubscribe();
		});
		expect(activeFilters).to.be.empty;
	});
});
