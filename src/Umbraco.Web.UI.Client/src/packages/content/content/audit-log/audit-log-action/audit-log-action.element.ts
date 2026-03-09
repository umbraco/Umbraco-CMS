import type { ManifestAuditLogAction } from './audit-log-action.extension.js';
import type { MetaAuditLogActionDefaultKind } from './types.js';
import { customElement, html, nothing, property, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbEntityAction, UmbEntityActionElement } from '@umbraco-cms/backoffice/entity-action';

@customElement('umb-audit-log-action')
export default class UmbAuditLogActionElement<
		MetaType extends MetaAuditLogActionDefaultKind = MetaAuditLogActionDefaultKind,
		ApiType extends UmbEntityAction<MetaType> = UmbEntityAction<MetaType>,
	>
	extends UmbLitElement
	implements UmbEntityActionElement
{
	#api?: ApiType;

	@property({ type: String })
	entityType?: string | null;

	@property({ type: String })
	public unique?: string | null;

	@property({ attribute: false })
	public manifest?: ManifestAuditLogAction<MetaType>;

	public set api(api: ApiType | undefined) {
		this.#api = api;
	}

	async #onClick(event: PointerEvent) {
		event.stopPropagation();

		try {
			await this.#api?.execute();
			this.dispatchEvent(new UmbActionExecutedEvent());
		} catch (error) {
			console.error('Error executing action:', error);
		}
	}

	override render() {
		if (!this.manifest) return nothing;
		const label = this.manifest.meta.label ? this.localize.string(this.manifest.meta.label) : this.manifest.name;
		return html`
			<uui-button
				compact
				data-mark=${'audit-log-action:' + this.manifest?.alias}
				label=${label}
				@click=${this.#onClick}>
				${when(this.manifest.meta.icon, (icon) => html`<umb-icon name=${icon}></umb-icon>`)}
				<span>${label}</span>
			</uui-button>
		`;
	}
}

export { UmbAuditLogActionElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-audit-log-action': UmbAuditLogActionElement;
	}
}
