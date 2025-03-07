import { UmbEntityContext } from '../../entity/entity.context.js';
import type { UmbEntityAction, ManifestEntityActionDefaultKind } from '@umbraco-cms/backoffice/entity-action';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { html, nothing, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import type { UmbSectionSidebarContext } from '@umbraco-cms/backoffice/section';
import { UMB_SECTION_SIDEBAR_CONTEXT } from '@umbraco-cms/backoffice/section';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbExtensionsManifestInitializer, createExtensionApi } from '@umbraco-cms/backoffice/extension-api';

@customElement('umb-entity-actions-bundle')
export class UmbEntityActionsBundleElement extends UmbLitElement {
	@property({ type: String, attribute: 'entity-type' })
	entityType?: string;

	@property({ type: String })
	unique?: string | null;

	@property({ type: String })
	public label?: string;

	@state()
	private _numberOfActions = 0;

	@state()
	private _firstActionManifest?: ManifestEntityActionDefaultKind;

	@state()
	private _firstActionApi?: UmbEntityAction<unknown>;

	@state()
	private _firstActionHref?: string;

	@state()
	_dropdownIsOpen = false;

	#sectionSidebarContext?: UmbSectionSidebarContext;

	// TODO: provide the entity context on a higher level, like the root element of this entity, tree-item/workspace/... [NL]
	#entityContext = new UmbEntityContext(this);

	constructor() {
		super();

		this.consumeContext(UMB_SECTION_SIDEBAR_CONTEXT, (sectionContext) => {
			this.#sectionSidebarContext = sectionContext;
		});
	}

	protected override updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		if (_changedProperties.has('entityType') && _changedProperties.has('unique')) {
			this.#entityContext.setEntityType(this.entityType);
			this.#entityContext.setUnique(this.unique ?? null);
			this.#observeEntityActions();
		}
	}

	#observeEntityActions() {
		new UmbExtensionsManifestInitializer(
			this,
			umbExtensionsRegistry,
			'entityAction',
			(ext) => ext.forEntityTypes.includes(this.entityType!),
			async (actions) => {
				this._numberOfActions = actions.length;
				this._firstActionManifest =
					this._numberOfActions > 0 ? (actions[0].manifest as ManifestEntityActionDefaultKind) : undefined;
				this.#createFirstActionApi();
			},
			'umbEntityActionsObserver',
		);
	}

	async #createFirstActionApi() {
		if (!this._firstActionManifest) return;
		this._firstActionApi = await createExtensionApi(this, this._firstActionManifest, [
			{ unique: this.unique, entityType: this.entityType, meta: this._firstActionManifest.meta },
		]);

		this._firstActionHref = await this._firstActionApi?.getHref();
	}

	#openContextMenu() {
		if (!this.entityType) throw new Error('Entity type is not defined');
		if (this.unique === undefined) throw new Error('Unique is not defined');

		if (this.#sectionSidebarContext) {
			this.#sectionSidebarContext.toggleContextMenu(this, {
				entityType: this.entityType,
				unique: this.unique,
				headline: this.label,
			});
		} else {
			this._dropdownIsOpen = !this._dropdownIsOpen;
		}
	}

	async #onFirstActionClick(event: PointerEvent) {
		this.#sectionSidebarContext?.closeContextMenu();

		// skip if href is defined
		if (this._firstActionHref) {
			return;
		}

		event.stopPropagation();
		try {
			await this._firstActionApi?.execute();
		} catch (error) {
			return;
		}
	}

	#onActionExecuted() {
		this._dropdownIsOpen = false;
	}

	#onDropdownClick(event: Event) {
		event.stopPropagation();
	}

	override render() {
		if (this._numberOfActions === 0) return nothing;
		return html`<uui-action-bar slot="actions">${this.#renderMore()} ${this.#renderFirstAction()} </uui-action-bar>`;
	}

	#renderMore() {
		if (this._numberOfActions === 1) return nothing;

		if (this.#sectionSidebarContext) {
			return html`<uui-button @click=${this.#openContextMenu} label="Open actions menu">
				<uui-symbol-more></uui-symbol-more>
			</uui-button>`;
		}

		return html`
			<umb-dropdown .open=${this._dropdownIsOpen} @click=${this.#onDropdownClick} compact hide-expand>
				<uui-symbol-more slot="label"></uui-symbol-more>
				<umb-entity-action-list
					@action-executed=${this.#onActionExecuted}
					.entityType=${this.entityType}
					.unique=${this.unique}></umb-entity-action-list>
			</umb-dropdown>
		`;
	}

	#renderFirstAction() {
		if (!this._firstActionApi) return nothing;
		return html`<uui-button
			label=${ifDefined(this._firstActionManifest?.meta.label)}
			@click=${this.#onFirstActionClick}
			href="${ifDefined(this._firstActionHref)}">
			<uui-icon name=${ifDefined(this._firstActionManifest?.meta.icon)}></uui-icon>
		</uui-button>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-actions-bundle': UmbEntityActionsBundleElement;
	}
}
