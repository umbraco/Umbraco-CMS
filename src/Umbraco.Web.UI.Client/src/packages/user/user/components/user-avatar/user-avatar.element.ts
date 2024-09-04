import type { UmbUserKindType } from '../../utils/index.js';
import { UmbUserKind } from '../../utils/index.js';
import { css, html, customElement, property, ifDefined, state, classMap } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const elementName = 'umb-user-avatar';
@customElement(elementName)
export class UmbUserAvatarElement extends UmbLitElement {
	@property({ type: String })
	name?: string;

	@property({ type: String })
	kind?: UmbUserKindType = UmbUserKind.DEFAULT;

	@property({ type: Array, attribute: false })
	public get imgUrls(): Array<string> {
		return this.#imgUrls;
	}
	public set imgUrls(value: Array<string>) {
		this.#imgUrls = value;
		this.#setUrls();
	}
	#imgUrls: Array<string> = [];

	@state()
	private _imgSrc: Array<{ scale: string; url: string }> = [];

	@state()
	private _imbSrcSet = '';

	@state()
	private hasImgUrls = false;

	#setUrls() {
		this._imbSrcSet = '';

		if (this.#imgUrls.length === 0) {
			this._imgSrc = [];
			this.hasImgUrls = false;
			return;
		}

		this._imgSrc = [
			{
				scale: '1x',
				url: this.#imgUrls[1],
			},
			{
				scale: '2x',
				url: this.#imgUrls[2],
			},
			{
				scale: '3x',
				url: this.#imgUrls[3],
			},
		];

		this._imgSrc.forEach((url) => (this._imbSrcSet += `${url.url} ${url.scale},`));
		this.hasImgUrls = true;
	}

	override render() {
		const classes = {
			default: this.kind === UmbUserKind.API,
			api: this.kind === UmbUserKind.API,
			'has-image': this.hasImgUrls,
		};

		return html`<uui-avatar
			.name=${this.name || 'Unknown'}
			img-src=${ifDefined(this.hasImgUrls ? this._imgSrc[0].url : undefined)}
			img-srcset=${ifDefined(this.hasImgUrls ? this._imbSrcSet : undefined)}
			class=${classMap(classes)}></uui-avatar>`;
	}

	static override styles = [
		css`
			uui-avatar {
				background-color: transparent;
				border: 1.5px solid var(--uui-color-divider-standalone);
			}

			uui-avatar.has-image {
				border-color: transparent;
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
