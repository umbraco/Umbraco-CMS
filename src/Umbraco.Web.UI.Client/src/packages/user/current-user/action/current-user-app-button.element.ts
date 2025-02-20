import type {
	ManifestCurrentUserActionDefaultKind,
	MetaCurrentUserActionDefaultKind,
	UmbCurrentUserAction,
} from '../current-user-action.extension.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, ifDefined, state, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-current-user-app-button')
export class UmbCurrentUserAppButtonElement<
	MetaType extends MetaCurrentUserActionDefaultKind = MetaCurrentUserActionDefaultKind,
	ApiType extends UmbCurrentUserAction<MetaType> = UmbCurrentUserAction<MetaType>,
> extends UmbLitElement {
	#api?: ApiType;

	@state()
	_href?: string;

	@property({ attribute: false })
	public manifest?: ManifestCurrentUserActionDefaultKind<MetaType>;

	public set api(api: ApiType | undefined) {
		this.#api = api;

		this.#api?.getHref?.().then((href) => {
			this._href = href;
		});
	}

	async #onClick(event: Event) {
		if (!this._href) {
			event.stopPropagation();
			await this.#api?.execute();
		}
		this.dispatchEvent(new UmbActionExecutedEvent());
	}

	get label(): string | undefined {
		return this.manifest?.meta.label ? this.localize.string(this.manifest.meta.label) : undefined;
	}

	override render() {
		return html`
			<uui-button
				@click=${this.#onClick}
				look="${this.manifest?.meta.look ?? 'primary'}"
				color="${this.manifest?.meta.color ?? 'default'}"
				label="${ifDefined(this.label)}"
				href="${ifDefined(this._href)}">
				${this.manifest?.meta.icon ? html`<uui-icon name="${this.manifest.meta.icon}"></uui-icon>` : ''} ${this.label}
			</uui-button>
		`;
	}

	static override styles = [UmbTextStyles];
}

export default UmbCurrentUserAppButtonElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-current-user-app-button': UmbCurrentUserAppButtonElement;
	}
}
