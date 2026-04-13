import type { ManifestBlockAction } from '../block-action.extension.js';
import type { UmbBlockAction } from '../block-action.interface.js';
import type { UmbBlockActionElement } from '../block-action-element.interface.js';
import { UMB_BLOCK_ENTRY_CONTEXT } from '../../context/block-entry.context-token.js';
import type { MetaBlockActionDefaultKind } from './types.js';
import { customElement, html, ifDefined, nothing, property, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-block-action')
export class UmbBlockActionDefaultElement<
		MetaType extends MetaBlockActionDefaultKind = MetaBlockActionDefaultKind,
		ApiType extends UmbBlockAction<MetaType> = UmbBlockAction<MetaType>,
	>
	extends UmbLitElement
	implements UmbBlockActionElement
{
	#api?: ApiType;

	@property({ attribute: false })
	public manifest?: ManifestBlockAction<MetaType>;

	public set api(api: ApiType | undefined) {
		this.#api = api;
		this.#api?.getHref?.().then((href) => {
			this._href = href;
		});
	}

	@state()
	private _href?: string;

	@state()
	private _isReadOnly = false;

	constructor() {
		super();

		this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, (context) => {
			if (!context) return;
			this.observe(
				context.readOnlyGuard.permitted,
				(isReadOnly) => (this._isReadOnly = isReadOnly),
				'observeIsReadOnly',
			);
		});
	}

	async #onClick(event: PointerEvent) {
		event.stopPropagation();

		if (this._href) return;

		try {
			await this.#api?.execute();
			this.dispatchEvent(new UmbActionExecutedEvent());
		} catch (error) {
			console.error('Error executing block action:', error);
		}
	}

	override render() {
		if (!this.manifest) return nothing;
		if (this._isReadOnly) return nothing;
		const label = this.manifest.meta.label ? this.localize.string(this.manifest.meta.label) : this.manifest.name;
		return html`
			<uui-button
				compact
				data-mark="block-action:${this.manifest.alias}"
				href=${ifDefined(this._href)}
				look="secondary"
				label=${label}
				title=${label}
				@click=${this.#onClick}>
				${when(this.manifest.meta.icon, (icon) => html`<uui-icon name=${icon}></uui-icon>`)}
			</uui-button>
		`;
	}
}

export default UmbBlockActionDefaultElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-action': UmbBlockActionDefaultElement;
	}
}
