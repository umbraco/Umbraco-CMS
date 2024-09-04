import type { UmbUserItemModel } from '../../repository/index.js';
import { css, html, customElement, property, nothing, ifDefined, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbUserKind } from '../../utils/index.js';

const elementName = 'umb-user-avatar';
@customElement(elementName)
export class UmbUserAvatarElement extends UmbLitElement {
	@property({ type: Object })
	public get user(): UmbUserItemModel | undefined {
		return this.#user;
	}
	public set user(value: UmbUserItemModel | undefined) {
		this.#user = value;
		this.#setUrls();
	}
	#user?: UmbUserItemModel | undefined;

	@state()
	private _urls: Array<{ scale: string; url: string }> = [];

	@state()
	private _srcset = '';

	#setUrls() {
		const urls = this.user?.avatarUrls ?? [];
		this._srcset = '';

		if (urls.length === 0) {
			this._urls = [];
			return;
		}

		this._urls = [
			{
				scale: '1x',
				url: urls[1],
			},
			{
				scale: '2x',
				url: urls[2],
			},
			{
				scale: '3x',
				url: urls[3],
			},
		];

		this._urls.forEach((url) => (this._srcset += `${url.url} ${url.scale},`));
	}

	override render() {
		if (!this.user) return nothing;

		return html`<uui-avatar
			.name=${this.user.name || 'Unknown'}
			img-src=${ifDefined(this._urls.length > 0 ? this._urls[0].url : undefined)}
			img-srcset=${ifDefined(this._urls.length > 0 ? this._srcset : undefined)}
			class="${this.user.kind === UmbUserKind.API ? 'api' : 'default'}"></uui-avatar>`;
	}

	static override styles = [
		css`
			uui-avatar {
				background-color: transparent;
				border: 1.5px solid var(--uui-color-divider-standalone);
			}

			uui-avatar.api {
				border-radius: var(--uui-border-radius);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbUserAvatarElement;
	}
}
