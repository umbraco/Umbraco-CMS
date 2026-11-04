import type { UmbEntityActionArgs } from './types.js';
import type { ManifestEntityAction, MetaEntityAction } from './entity-action.extension.js';
import { UmbEntityContext, UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { html, customElement, property, query, state, css } from '@umbraco-cms/backoffice/external/lit';
import type { PropertyValues } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbExtensionsElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import type { UmbExtensionElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';

const umbEntityActionListDeprecation = new UmbDeprecation({
	deprecated: 'The `entityType` and `unique` properties on `<umb-entity-action-list>`.',
	removeInVersion: '19',
	solution: 'Provide the entity type and unique via the UMB_ENTITY_CONTEXT context instead.',
});

@customElement('umb-entity-action-list')
export class UmbEntityActionListElement extends UmbLitElement {
	/**
	 * @deprecated Provide through the UMB_ENTITY_CONTEXT context instead. Will be removed in Umbraco 19.
	 * @returns {string | undefined} The entity type.
	 */
	@property({ type: String, attribute: 'entity-type' })
	public get entityType(): string | undefined {
		return this._props.entityType;
	}
	public set entityType(value: string | undefined) {
		if (value === undefined || value === this._props.entityType) return;
		umbEntityActionListDeprecation.warn();
		this._props.entityType = value;
		this.#ensureFallbackEntityContext().setEntityType(value);
		this.#generateApiArgs();
	}

	/**
	 * @deprecated Provide through the UMB_ENTITY_CONTEXT context instead. Will be removed in Umbraco 19.
	 * @returns {string | null | undefined} The unique key.
	 */
	@property({ type: String })
	public get unique(): string | null | undefined {
		return this._props.unique;
	}
	public set unique(value: string | null | undefined) {
		if (value === this._props.unique) return;
		umbEntityActionListDeprecation.warn();
		this._props.unique = value;
		this.#ensureFallbackEntityContext().setUnique(value ?? null);
		this.#generateApiArgs();
	}

	@state()
	private _props: Partial<UmbEntityActionArgs<unknown>> = {};

	@state()
	private _actions: Array<UmbExtensionElementAndApiInitializer<ManifestEntityAction>> = [];

	@query('umb-grouped-action-list')
	private _actionList?: HTMLElementTagNameMap['umb-grouped-action-list'];

	#fallbackEntityContext?: UmbEntityContext;

	// Actions appear as their conditions resolve, so the first action keeps focus until the user takes over.
	#followFirstAction = false;

	#stopFollowingFirstAction = () => {
		this.#followFirstAction = false;
	};

	constructor() {
		super();
		this.addEventListener('keydown', this.#stopFollowingFirstAction);
		this.addEventListener('pointerdown', this.#stopFollowingFirstAction);

		this.consumeContext(UMB_ENTITY_CONTEXT, (context) => {
			if (!context) return;
			this.observe(observeMultiple([context.entityType, context.unique]), ([entityType, unique]) => {
				this._props.entityType = entityType ?? undefined;
				this._props.unique = unique;
				this.#generateApiArgs();
			});
		});
	}

	// TODO: v19 remove when fallback context is no longer needed
	#ensureFallbackEntityContext(): UmbEntityContext {
		if (!this.#fallbackEntityContext) {
			this.#fallbackEntityContext = new UmbEntityContext(this);
		}
		return this.#fallbackEntityContext;
	}

	#generateApiArgs() {
		const { entityType, unique } = this._props;
		if (!entityType || unique === undefined) return;

		this.#followFirstAction = true;
		const initializer = new UmbExtensionsElementAndApiInitializer(
			this,
			umbExtensionsRegistry,
			'entityAction',
			(manifest: ManifestEntityAction<MetaEntityAction>) => [{ entityType, unique, meta: manifest.meta }],
			(manifest: ManifestEntityAction<MetaEntityAction>) => manifest.forEntityTypes.includes(entityType),
			(actions) => (this._actions = actions),
			'umbEntityActionsInitializer',
		);
		initializer.elementProperties = { entityType, unique };
	}

	override focus() {
		this._actionList?.focus();
	}

	protected override async updated(changedProperties: PropertyValues) {
		super.updated(changedProperties);
		if (!this.#followFirstAction || !changedProperties.has('_actions') || !this._actionList) return;
		await this._actionList.updateComplete;
		if (this.#followFirstAction) this._actionList.focus();
	}

	override render() {
		return html`<umb-grouped-action-list
			group-type="entityActionGroup"
			.actions=${this._actions}></umb-grouped-action-list>`;
	}

	static override styles = [
		css`
			:host {
				--uui-menu-item-indent: 0;
				--uui-menu-item-flat-structure: 1;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-action-list': UmbEntityActionListElement;
	}
}
