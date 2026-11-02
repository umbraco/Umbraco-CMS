import { UMB_ENTITY_NAMED_DETAIL_WORKSPACE_CONTEXT } from '../../../entity-detail/index.js';
import { UMB_VARIANT_WORKSPACE_CONTEXT } from '../../../contexts/index.js';
import type { UmbWorkspaceVariantMenuBreadcrumbElement } from './workspace-variant-menu-breadcrumb.element.js';
import './workspace-variant-menu-breadcrumb.element.js';
import { UMB_MENU_VARIANT_STRUCTURE_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/menu';
import type { UmbVariantStructureItemModel } from '@umbraco-cms/backoffice/menu';
import { UMB_APP_LANGUAGE_CONTEXT } from '@umbraco-cms/backoffice/language';
import { expect, waitUntil } from '@open-wc/testing';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbArrayState, UmbObjectState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { ignoreResizeObserverLoopErrors } from '@umbraco-cms/internal/test-utils';

@customElement('umb-test-breadcrumb-host')
class UmbTestBreadcrumbHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

class UmbTestAppLanguageContext {
	#host: UmbControllerHost;
	#defaultLanguage = new UmbObjectState<{ unique: string } | undefined>(undefined);
	#currentCulture = new UmbStringState<string | undefined>(undefined);
	readonly appDefaultLanguage = this.#defaultLanguage.asObservable();
	readonly appLanguageCulture = this.#currentCulture.asObservable();

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	getHostElement() {
		return this.#host.getHostElement();
	}

	setDefaultCulture(culture: string | undefined) {
		this.#defaultLanguage.setValue(culture ? { unique: culture } : undefined);
	}

	setCurrentCulture(culture: string | undefined) {
		this.#currentCulture.setValue(culture);
	}
}

/** Satisfies UMB_MENU_VARIANT_STRUCTURE_WORKSPACE_CONTEXT's discriminator (`IS_MENU_VARIANT_STRUCTURE_WORKSPACE_CONTEXT` present). */
class UmbTestMenuStructureContext {
	readonly IS_MENU_VARIANT_STRUCTURE_WORKSPACE_CONTEXT = true;
	#host: UmbControllerHost;
	#structure = new UmbArrayState<UmbVariantStructureItemModel>([], (x) => x.unique);
	readonly structure = this.#structure.asObservable();
	readonly hrefsByUnique = new Map<string | null, string>();

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	getHostElement() {
		return this.#host.getHostElement();
	}

	setStructure(items: Array<UmbVariantStructureItemModel>) {
		this.#structure.setValue(items);
	}

	getItemHref(item: UmbVariantStructureItemModel) {
		return this.hrefsByUnique.get(item.unique);
	}
}

/** Satisfies UMB_VARIANT_WORKSPACE_CONTEXT's discriminator (`'variants' in context`). */
class UmbTestVariantWorkspaceContext {
	readonly variants: Array<unknown> = [];
	#host: UmbControllerHost;
	#unique: string | null = null;
	#name = new UmbStringState('');
	#activeVariantInfo = new UmbObjectState<{ culture: string | null; segment: string | null } | undefined>(undefined);
	readonly splitView = { firstActiveVariantInfo: this.#activeVariantInfo.asObservable() };

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	getHostElement() {
		return this.#host.getHostElement();
	}

	getUnique() {
		return this.#unique;
	}

	setUnique(unique: string | null) {
		this.#unique = unique;
	}

	setActiveVariant(culture: string | null, segment: string | null = null) {
		this.#activeVariantInfo.setValue({ culture, segment });
	}

	name() {
		return this.#name.asObservable();
	}

	setName(name: string) {
		this.#name.setValue(name);
	}
}

/** Satisfies UMB_ENTITY_NAMED_DETAIL_WORKSPACE_CONTEXT's discriminator (`IS_ENTITY_NAMED_DETAIL_WORKSPACE_CONTEXT` truthy). */
class UmbTestNamedDetailWorkspaceContext {
	readonly IS_ENTITY_NAMED_DETAIL_WORKSPACE_CONTEXT = true;
	#host: UmbControllerHost;
	#unique: string | null = null;
	#name = new UmbStringState<string | undefined>(undefined);
	readonly name = this.#name.asObservable();

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	getHostElement() {
		return this.#host.getHostElement();
	}

	getUnique() {
		return this.#unique;
	}

	setUnique(unique: string | null) {
		this.#unique = unique;
	}

	setName(name: string | undefined) {
		this.#name.setValue(name);
	}
}

function structureItem(overrides: Partial<UmbVariantStructureItemModel>): UmbVariantStructureItemModel {
	return {
		unique: 'item-unique',
		entityType: 'test-entity-type',
		variants: [],
		...overrides,
	};
}

describe('UmbWorkspaceVariantMenuBreadcrumbElement', () => {
	let host: UmbTestBreadcrumbHostElement;
	let element: UmbWorkspaceVariantMenuBreadcrumbElement;
	let menuStructureContext: UmbTestMenuStructureContext;
	let appLanguageContext: UmbTestAppLanguageContext;
	let restoreErrorHandler: () => void;

	function getItems() {
		return [...(element.shadowRoot?.querySelectorAll('uui-breadcrumb-item') ?? [])];
	}

	function getAncestorItems() {
		return getItems().filter((item) => !item.hasAttribute('last-item'));
	}

	function getLastItem() {
		return getItems().find((item) => item.hasAttribute('last-item'));
	}

	async function render(options: {
		variantWorkspace?: UmbTestVariantWorkspaceContext;
		namedDetailWorkspace?: UmbTestNamedDetailWorkspaceContext;
	}) {
		new UmbContextProviderController(host, UMB_MENU_VARIANT_STRUCTURE_WORKSPACE_CONTEXT, menuStructureContext as never);
		new UmbContextProviderController(host, UMB_APP_LANGUAGE_CONTEXT, appLanguageContext as never);
		if (options.variantWorkspace) {
			new UmbContextProviderController(host, UMB_VARIANT_WORKSPACE_CONTEXT, options.variantWorkspace as never);
		}
		if (options.namedDetailWorkspace) {
			new UmbContextProviderController(
				host,
				UMB_ENTITY_NAMED_DETAIL_WORKSPACE_CONTEXT,
				options.namedDetailWorkspace as never,
			);
		}

		element = document.createElement('umb-workspace-variant-menu-breadcrumb') as UmbWorkspaceVariantMenuBreadcrumbElement;
		host.appendChild(element);

		await waitUntil(() => !!element.shadowRoot?.querySelector('uui-breadcrumbs'));
		await element.updateComplete;
	}

	beforeEach(() => {
		restoreErrorHandler = ignoreResizeObserverLoopErrors();
		host = document.createElement('umb-test-breadcrumb-host') as UmbTestBreadcrumbHostElement;
		document.body.appendChild(host);
		menuStructureContext = new UmbTestMenuStructureContext(host);
		appLanguageContext = new UmbTestAppLanguageContext(host);
	});

	afterEach(() => {
		document.body.removeChild(host);
		restoreErrorHandler();
	});

	describe('variant workspace', () => {
		let variantWorkspace: UmbTestVariantWorkspaceContext;

		beforeEach(() => {
			variantWorkspace = new UmbTestVariantWorkspaceContext(host);
			variantWorkspace.setUnique('current-unique');
			variantWorkspace.setName('Current Item');
			appLanguageContext.setCurrentCulture('en');
			appLanguageContext.setDefaultCulture('fr');
		});

		describe('ancestor name resolution', () => {
			it('uses the exact active-variant match, unparenthesized', async () => {
				variantWorkspace.setActiveVariant('da');
				menuStructureContext.setStructure([
					structureItem({ unique: 'a', variants: [{ name: 'Item DA', culture: 'da', segment: null }] }),
				]);
				await render({ variantWorkspace });

				expect(getAncestorItems()[0].textContent?.trim()).to.equal('Item DA');
			});

			it('falls back to the app current culture, parenthesized, when the active variant is not invariant', async () => {
				variantWorkspace.setActiveVariant('da');
				menuStructureContext.setStructure([
					// A decoy entry first, so this only passes if the "en" one was picked by culture match -
					// not by falling through to the first-variant last resort (which would also parenthesize).
					structureItem({
						unique: 'a',
						variants: [
							{ name: 'Decoy', culture: 'zz', segment: null },
							{ name: 'Item EN', culture: 'en', segment: null },
						],
					}),
				]);
				await render({ variantWorkspace });

				expect(getAncestorItems()[0].textContent?.trim()).to.equal('(Item EN)');
			});

			it('uses the app current culture unparenthesized when the active variant is invariant', async () => {
				variantWorkspace.setActiveVariant(null);
				menuStructureContext.setStructure([
					structureItem({ unique: 'a', variants: [{ name: 'Item EN', culture: 'en', segment: null }] }),
				]);
				await render({ variantWorkspace });

				expect(getAncestorItems()[0].textContent?.trim()).to.equal('Item EN');
			});

			it('falls back to the app default culture, parenthesized', async () => {
				variantWorkspace.setActiveVariant('da');
				menuStructureContext.setStructure([
					// A decoy entry first, so this only passes if the "fr" one was picked by culture match -
					// not by falling through to the first-variant last resort (which would also parenthesize).
					structureItem({
						unique: 'a',
						variants: [
							{ name: 'Decoy', culture: 'zz', segment: null },
							{ name: 'Item FR', culture: 'fr', segment: null },
						],
					}),
				]);
				await render({ variantWorkspace });

				expect(getAncestorItems()[0].textContent?.trim()).to.equal('(Item FR)');
			});

			it('falls back to the invariant variant, unparenthesized', async () => {
				variantWorkspace.setActiveVariant('da');
				menuStructureContext.setStructure([
					structureItem({ unique: 'a', variants: [{ name: 'Item Invariant', culture: null, segment: null }] }),
				]);
				await render({ variantWorkspace });

				expect(getAncestorItems()[0].textContent?.trim()).to.equal('Item Invariant');
			});

			it('falls back to the first variant, parenthesized, when nothing else matches', async () => {
				variantWorkspace.setActiveVariant('da');
				menuStructureContext.setStructure([
					structureItem({ unique: 'a', variants: [{ name: 'ZZ Name', culture: 'zz', segment: null }] }),
				]);
				await render({ variantWorkspace });

				expect(getAncestorItems()[0].textContent?.trim()).to.equal('(ZZ Name)');
			});

			it('renders the general-unknown placeholder when there are no variants at all', async () => {
				variantWorkspace.setActiveVariant('da');
				menuStructureContext.setStructure([structureItem({ unique: 'a', variants: [] })]);
				await render({ variantWorkspace });

				expect(getAncestorItems()[0].textContent?.trim()).to.equal('(#general_unknown)');
			});

			it('renders an isFolder item by its flat name, bypassing variant matching entirely', async () => {
				variantWorkspace.setActiveVariant('da');
				menuStructureContext.setStructure([
					structureItem({ unique: 'a', isFolder: true, name: 'Some Folder', variants: [] }),
				]);
				await render({ variantWorkspace });

				expect(getAncestorItems()[0].textContent?.trim()).to.equal('Some Folder');
			});
		});

		it('renders the last segment from the variant workspace live name, updating as it changes', async () => {
			variantWorkspace.setActiveVariant('da');
			menuStructureContext.setStructure([]);
			await render({ variantWorkspace });

			expect(getLastItem()?.textContent?.trim()).to.equal('Current Item');

			variantWorkspace.setName('Renamed Item');
			await waitUntil(() => getLastItem()?.textContent?.trim() === 'Renamed Item');
		});

		it('excludes the current item from the ancestor list', async () => {
			variantWorkspace.setActiveVariant('da');
			menuStructureContext.setStructure([
				structureItem({ unique: 'ancestor', variants: [{ name: 'Ancestor', culture: 'da', segment: null }] }),
				structureItem({ unique: 'current-unique', variants: [{ name: 'Current Item', culture: 'da', segment: null }] }),
			]);
			await render({ variantWorkspace });

			expect(getAncestorItems()).to.have.lengthOf(1);
			expect(getAncestorItems()[0].textContent?.trim()).to.equal('Ancestor');
		});

		it('sets each ancestor href from the menu structure context', async () => {
			variantWorkspace.setActiveVariant('da');
			menuStructureContext.hrefsByUnique.set('a', '/test/a');
			menuStructureContext.setStructure([
				structureItem({ unique: 'a', variants: [{ name: 'Item A', culture: 'da', segment: null }] }),
				structureItem({ unique: 'b', variants: [{ name: 'Item B', culture: 'da', segment: null }] }),
			]);
			await render({ variantWorkspace });

			const items = getAncestorItems();
			expect(items[0].getAttribute('href')).to.equal('/test/a');
			expect(items[1].hasAttribute('href')).to.be.false;
		});
	});

	describe('non-variant workspace (e.g. a folder)', () => {
		let namedDetailWorkspace: UmbTestNamedDetailWorkspaceContext;

		beforeEach(() => {
			namedDetailWorkspace = new UmbTestNamedDetailWorkspaceContext(host);
			namedDetailWorkspace.setUnique('folder-unique');
			namedDetailWorkspace.setName('My Folder');
		});

		it('renders the last segment from the named-detail context, since there is no variant context', async () => {
			menuStructureContext.setStructure([]);
			await render({ namedDetailWorkspace });

			expect(getLastItem()?.textContent?.trim()).to.equal('My Folder');
		});

		it('updates the last segment live as the name changes', async () => {
			menuStructureContext.setStructure([]);
			await render({ namedDetailWorkspace });

			namedDetailWorkspace.setName('Renamed Folder');
			await waitUntil(() => getLastItem()?.textContent?.trim() === 'Renamed Folder');
		});

		it('excludes the current item from the ancestor list using the named-detail context unique', async () => {
			menuStructureContext.setStructure([
				structureItem({ unique: 'ancestor', isFolder: true, name: 'Ancestor Folder', variants: [] }),
				structureItem({ unique: 'folder-unique', isFolder: true, name: 'My Folder', variants: [] }),
			]);
			await render({ namedDetailWorkspace });

			expect(getAncestorItems()).to.have.lengthOf(1);
			expect(getAncestorItems()[0].textContent?.trim()).to.equal('Ancestor Folder');
		});
	});
});
