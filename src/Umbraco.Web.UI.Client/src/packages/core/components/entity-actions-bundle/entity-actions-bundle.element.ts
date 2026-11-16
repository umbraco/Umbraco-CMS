import { umbOrderActionsByGroup } from '@umbraco-cms/backoffice/action';
import type { ManifestActionGroupBase } from '@umbraco-cms/backoffice/action';
import { UmbEntityContext, UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { customElement, html, ifDefined, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbExtensionsManifestInitializer, createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbExtensionManifestInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type {
	UmbEntityAction,
	ManifestEntityAction,
	ManifestEntityActionDefaultKind,
} from '@umbraco-cms/backoffice/entity-action';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';
import type { UUIInterfaceLook } from '@umbraco-cms/backoffice/external/uui';

const umbEntityActionsBundleDeprecation = new UmbDeprecation({
	deprecated: 'The `entityType` and `unique` properties on `<umb-entity-actions-bundle>`.',
	removeInVersion: '19',
	solution: 'Provide the entity type and unique via the UMB_ENTITY_CONTEXT context instead.',
});

@customElement('umb-entity-actions-bundle')
export class UmbEntityActionsBundleElement extends UmbLitElement {
	/**
	 * @deprecated Provide through the UMB_ENTITY_CONTEXT context instead. Will be removed in Umbraco 19.
	 * @returns {string | undefined} The entity type.
	 */
	@property({ type: String, attribute: 'entity-type' })
	get entityType(): string | undefined {
		return this.#entityType;
	}
	set entityType(value: string | undefined) {
		if (value === this.#entityType) return;
		umbEntityActionsBundleDeprecation.warn();
		this.#entityType = value;
		this.#ensureFallbackEntityContext().setEntityType(value);
		this.#requestObserveEntityActions();
	}

	/**
	 * @deprecated Provide through the UMB_ENTITY_CONTEXT context instead. Will be removed in Umbraco 19.
	 * @returns {string | null | undefined} The unique key.
	 */
	@property({ type: String })
	get unique(): string | null | undefined {
		return this.#unique;
	}
	set unique(value: string | null | undefined) {
		if (value === this.#unique) return;
		umbEntityActionsBundleDeprecation.warn();
		this.#unique = value;
		this.#ensureFallbackEntityContext().setUnique(value ?? null);
		this.#requestObserveEntityActions();
	}

	@property({ type: String })
	public label?: string;

	@property({ type: String })
	/**
	 * The look applied to the action buttons, both the first action and the "more" dropdown.
	 * @type {UUIInterfaceLook}
	 */
	@property({ type: String })
	public look?: UUIInterfaceLook;

	@state()
	private _numberOfActions = 0;

	@state()
	private _firstActionManifest?: ManifestEntityActionDefaultKind;

	@state()
	private _firstActionApi?: UmbEntityAction<unknown>;

	@state()
	private _firstActionHref?: string;

	#entityType?: string;
	#unique?: string | null;
	#fallbackEntityContext?: UmbEntityContext;
	#inViewport = false;
	#observingEntityActions = false;
	#actions: Array<UmbExtensionManifestInitializer<ManifestEntityAction>> = [];
	#groups: Array<ManifestActionGroupBase> = [];
	#firstActionRequest = 0;
	#requestedFirstActionManifest?: ManifestEntityActionDefaultKind;

	constructor() {
		super();

		this.observe(
			umbExtensionsRegistry.byType('entityActionGroup'),
			(groups) => {
				this.#groups = groups;
				if (this.#actions.length > 0) void this.#updateFirstAction(true);
			},
			null,
		);

		this.consumeContext(UMB_ENTITY_CONTEXT, (context) => {
			if (!context) return;
			this.observe(observeMultiple([context.entityType, context.unique]), ([entityType, unique]) => {
				this.#entityType = entityType ?? undefined;
				this.#unique = unique;
				this.#requestObserveEntityActions();
			});
		});

		// Only observe entity actions when the element is in the viewport
		const observer = new IntersectionObserver(
			(entries) => {
				entries.forEach((entry) => {
					if (entry.isIntersecting) {
						this.#inViewport = true;
						this.#requestObserveEntityActions();
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

	// TODO: v19 remove when fallback context is no longer needed
	#ensureFallbackEntityContext(): UmbEntityContext {
		if (!this.#fallbackEntityContext) {
			this.#fallbackEntityContext = new UmbEntityContext(this);
		}
		return this.#fallbackEntityContext;
	}

	#requestObserveEntityActions() {
		if (!this.#entityType) return;
		if (this.#unique === undefined) return;
		if (!this.#inViewport) return; // Only observe if the element is in the viewport
		if (this.#observingEntityActions) return;

		new UmbExtensionsManifestInitializer(
			this,
			umbExtensionsRegistry,
			'entityAction',
			(ext) => ext.forEntityTypes.includes(this.#entityType!),
			async (actions) => {
				this.#actions = actions;
				await this.#updateFirstAction();
			},
			'umbEntityActionsObserver',
		);

		this.#observingEntityActions = true;
	}

	async #updateFirstAction(onlyWhenChanged = false) {
		this._numberOfActions = this.#actions.length;
		// The inline action is the first action of the menu, so it follows the action groups.
		const [firstBlock] = umbOrderActionsByGroup(this.#actions, this.#groups);
		const manifest = firstBlock?.actions[0].manifest as ManifestEntityActionDefaultKind | undefined;
		if (onlyWhenChanged && manifest === this.#requestedFirstActionManifest) return;
		this.#requestedFirstActionManifest = manifest;
		const request = ++this.#firstActionRequest;
		const api = manifest ? await this.#createFirstActionApi(manifest) : undefined;
		const href = await api?.getHref();
		// Only the latest request publishes, and it publishes the manifest, API and href together.
		if (request !== this.#firstActionRequest) {
			api?.destroy();
			return;
		}
		this._firstActionManifest = manifest;
		this._firstActionApi = api;
		this._firstActionHref = href;
	}

	async #createFirstActionApi(manifest: ManifestEntityActionDefaultKind) {
		const api = await createExtensionApi(this, manifest, [
			{ unique: this.#unique, entityType: this.#entityType, meta: manifest.meta },
		]);
		if (api) {
			(api as any).manifest = manifest;
		}
		return api;
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
		return html`<uui-action-bar slot="actions">${this.#renderMore()}${this.#renderFirstAction()}</uui-action-bar>`;
	}

	#renderMore() {
		if (this._numberOfActions === 1) return nothing;
		return html`
			<umb-entity-actions-dropdown
				look=${ifDefined(this.look)}
				compact
				.label=${this.localize.term('actions_viewActionsFor', this.label)}>
				<uui-symbol-more slot="label"></uui-symbol-more>
			</umb-entity-actions-dropdown>
		`;
	}

	#renderFirstAction() {
		if (!this._firstActionApi || !this._firstActionManifest) return nothing;
		const label = this.localize.string(this._firstActionManifest.meta.label, this.label);
		return html`
			<uui-button
				label=${label}
				title=${label}
				look=${ifDefined(this.look)}
				data-mark=${`entity-action:${this._firstActionManifest.alias}`}
				href=${ifDefined(this._firstActionHref)}
				@click=${this.#onFirstActionClick}>
				<umb-icon name=${ifDefined(this._firstActionManifest.meta.icon)}></umb-icon>
			</uui-button>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-actions-bundle': UmbEntityActionsBundleElement;
	}
}
