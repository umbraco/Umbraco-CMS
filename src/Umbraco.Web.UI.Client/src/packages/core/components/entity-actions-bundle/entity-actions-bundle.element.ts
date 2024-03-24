import type { UmbEntityAction } from '@umbraco-cms/backoffice/entity-action';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { html, nothing, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import type { UmbSectionSidebarContext } from '@umbraco-cms/backoffice/section';
import { UMB_SECTION_SIDEBAR_CONTEXT } from '@umbraco-cms/backoffice/section';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestEntityActionDefaultKind } from '@umbraco-cms/backoffice/extension-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';

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

	#sectionSidebarContext?: UmbSectionSidebarContext;

	constructor() {
		super();

		this.consumeContext(UMB_SECTION_SIDEBAR_CONTEXT, (sectionContext) => {
			this.#sectionSidebarContext = sectionContext;
		});
	}

	protected updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		if (_changedProperties.has('entityType') && _changedProperties.has('unique')) {
			this.#observeEntityActions();
		}
	}

	#observeEntityActions() {
		this.observe(
			umbExtensionsRegistry.byType('entityAction'),
			async (manifests) => {
				const actions = manifests.filter((manifest) => manifest.forEntityTypes.includes(this.entityType!));
				this._numberOfActions = actions.length;
				this._firstActionManifest =
					this._numberOfActions > 0 ? (actions[0] as ManifestEntityActionDefaultKind) : undefined;
				if (!this._firstActionManifest) return;
				this._firstActionApi = await createExtensionApi(this, this._firstActionManifest, [
					{ unique: this.unique, entityType: this.entityType },
				]);
			},
			'umbEntityActionsObserver',
		);
	}

	#openContextMenu() {
		if (!this.entityType) throw new Error('Entity type is not defined');
		if (this.unique === undefined) throw new Error('Unique is not defined');
		this.#sectionSidebarContext?.toggleContextMenu(this.entityType, this.unique, this.label);
	}

	async #onFirstActionClick(event: PointerEvent) {
		event.stopPropagation();
		await this._firstActionApi?.execute();
	}

	render() {
		if (this._numberOfActions === 0) return nothing;
		return html`<uui-action-bar slot="actions"> ${this.#renderFirstAction()} ${this.#renderMore()} </uui-action-bar>`;
	}

	#renderMore() {
		if (this._numberOfActions === 1) return nothing;
		return html`<uui-button @click=${this.#openContextMenu} label="Open actions menu">
			<uui-symbol-more></uui-symbol-more>
		</uui-button>`;
	}

	#renderFirstAction() {
		if (!this._firstActionApi) return nothing;
		return html`<uui-button
			label=${ifDefined(this._firstActionManifest?.meta.label)}
			@click=${this.#onFirstActionClick}>
			<uui-icon name=${ifDefined(this._firstActionManifest?.meta.icon)}></uui-icon>
		</uui-button>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-actions-bundle': UmbEntityActionsBundleElement;
	}
}
