import { UmbEntityContext } from '../../entity/entity.context.js';
import type { UmbEntityAction, ManifestEntityActionDefaultKind } from '@umbraco-cms/backoffice/entity-action';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { html, nothing, customElement, property, state, ifDefined, css } from '@umbraco-cms/backoffice/external/lit';
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

	// TODO: provide the entity context on a higher level, like the root element of this entity, tree-item/workspace/... [NL]
	#entityContext = new UmbEntityContext(this);
	#inViewport = false;
	#observingEntityActions = false;

	constructor() {
		super();

		// Only observe entity actions when the element is in the viewport
		const observer = new IntersectionObserver(
			(entries) => {
				entries.forEach((entry) => {
					if (entry.isIntersecting) {
						this.#inViewport = true;
						this.#observeEntityActions();
					}
				});
			},
			{
				root: null, // Use the viewport as the root
				threshold: 0.1, // Trigger when at least 10% of the element is visible
			},
		);

		observer.observe(this);
	}

	protected override updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		if (_changedProperties.has('entityType') && _changedProperties.has('unique')) {
			this.#entityContext.setEntityType(this.entityType);
			this.#entityContext.setUnique(this.unique ?? null);
			this.#observeEntityActions();
		}
	}

	#observeEntityActions() {
		if (!this.entityType) return;
		if (this.unique === undefined) return;
		if (!this.#inViewport) return; // Only observe if the element is in the viewport
		if (this.#observingEntityActions) return;

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

		this.#observingEntityActions = true;
	}

	async #createFirstActionApi() {
		if (!this._firstActionManifest) return;
		this._firstActionApi = await createExtensionApi(this, this._firstActionManifest, [
			{ unique: this.unique, entityType: this.entityType, meta: this._firstActionManifest.meta },
		]);
		if (this._firstActionApi) {
			(this._firstActionApi as any).manifest = this._firstActionManifest;
			this._firstActionHref = await this._firstActionApi.getHref();
		}
	}

	async #onFirstActionClick(event: PointerEvent) {
		// skip if href is defined
		if (this._firstActionHref) {
			return;
		}

		event.stopPropagation();
		await this._firstActionApi?.execute().catch(() => {});
	}

	override render() {
		if (this._numberOfActions === 0) return nothing;
		return html`<uui-action-bar slot="actions">${this.#renderMore()} ${this.#renderFirstAction()} </uui-action-bar>`;
	}

	#renderMore() {
		if (this._numberOfActions === 1) return nothing;

		return html`
			<umb-entity-actions-dropdown .label=${this.label} compact>
				<uui-symbol-more slot="label"></uui-symbol-more>
			</umb-entity-actions-dropdown>
		`;
	}

	#renderFirstAction() {
		if (!this._firstActionApi || !this._firstActionManifest) return nothing;
		return html`<uui-button
			label=${this.localize.string(this._firstActionManifest.meta.label)}
			data-mark=${'entity-action:' + this._firstActionManifest.alias}
			@click=${this.#onFirstActionClick}
			href="${ifDefined(this._firstActionHref)}">
			<uui-icon name=${ifDefined(this._firstActionManifest.meta.icon)}></uui-icon>
		</uui-button>`;
	}

	static override styles = [
		css`
			uui-scroll-container {
				max-height: 700px;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-actions-bundle': UmbEntityActionsBundleElement;
	}
}
