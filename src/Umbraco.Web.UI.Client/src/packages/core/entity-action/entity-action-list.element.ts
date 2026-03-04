import type { UmbEntityActionArgs } from './types.js';
import type { ManifestEntityAction, MetaEntityAction } from './entity-action.extension.js';
import { UmbEntityContext, UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { html, customElement, property, state, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbApiConstructorArgumentsMethodType } from '@umbraco-cms/backoffice/extension-api';
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
	private _filter?: (extension: ManifestEntityAction<MetaEntityAction>) => boolean;

	@state()
	private _props: Partial<UmbEntityActionArgs<unknown>> = {};

	@state()
	private _apiArgs?: UmbApiConstructorArgumentsMethodType<
		ManifestEntityAction<MetaEntityAction>,
		[UmbEntityActionArgs<MetaEntityAction>]
	>;

	#fallbackEntityContext?: UmbEntityContext;

	constructor() {
		super();

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
		if (!this._props.entityType || this._props.unique === undefined) return;

		this._filter = (extension: ManifestEntityAction<MetaEntityAction>) =>
			extension.forEntityTypes.includes(this._props.entityType!);
		this.#hasRenderedOnce = false;

		this._apiArgs = (manifest: ManifestEntityAction<MetaEntityAction>) => {
			return [{ entityType: this._props.entityType!, unique: this._props.unique!, meta: manifest.meta }];
		};
	}

	override focus() {
		this.#firstEntityAction?.focus();
	}

	#firstEntityAction?: HTMLElement;

	#hasRenderedOnce?: boolean;
	override render() {
		return this._filter
			? html`
					<umb-extension-with-api-slot
						type="entityAction"
						.filter=${this._filter}
						.elementProps=${this._props}
						.apiArgs=${this._apiArgs}
						.renderMethod=${(ext: any, i: number) => {
							if (!this.#hasRenderedOnce && i === 0) {
								this.#firstEntityAction = ext.component;
								this.#firstEntityAction?.focus();
								this.#hasRenderedOnce = true;
							}
							return ext.component;
						}}></umb-extension-with-api-slot>
				`
			: '';
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
