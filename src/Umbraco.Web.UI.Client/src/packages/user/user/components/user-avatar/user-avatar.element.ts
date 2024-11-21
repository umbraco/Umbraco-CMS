import type { UmbUserKindType } from '../../utils/index.js';
import { UmbUserKind } from '../../utils/index.js';
import type { UUIAvatarElement } from '@umbraco-cms/backoffice/external/uui';
import {
	css,
	html,
	customElement,
	property,
	ifDefined,
	state,
	classMap,
	query,
} from '@umbraco-cms/backoffice/external/lit';
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
		this.hasImgUrls = value.length > 0;
		this.#setImgSrcSizes();
	}
	#imgUrls: Array<string> = [];

	@state()
	private _imgSrcSizes: Array<{ w: number; url: string }> = [];

	@state()
	private _imgSrc = '';

	@state()
	private hasImgUrls = false;

	@query('uui-avatar')
	avatarElement!: UUIAvatarElement;

	#setImgSrcSizes() {
		if (this.#imgUrls.length === 0) {
			this._imgSrcSizes = [];
			return;
		}

		this._imgSrcSizes = [
			{
				w: 30,
				url: this.#imgUrls[0],
			},
			{
				w: 60,
				url: this.#imgUrls[1],
			},
			{
				w: 90,
				url: this.#imgUrls[2],
			},
			{
				w: 150,
				url: this.#imgUrls[3],
			},
			{
				w: 300,
				url: this.#imgUrls[4],
			},
		];

		this.#setImgSrc();
	}

	protected override firstUpdated(): void {
		this.#setImgSrc();
	}

	async #setImgSrc() {
		if (!this.hasImgUrls) return;
		if (!this.avatarElement) return;

		setTimeout(() => {
			// TODO: look into img sizes="auto" to let the browser handle the correct image size based on the element size
			const elementSize = this.avatarElement.getBoundingClientRect();
			const elementWidth = elementSize.width;

			const matchingSizes = this._imgSrcSizes.filter((size) => {
				// we multiply the element width to make sure we have a good quality image
				return elementWidth * 1.5 <= size.w;
			});

			// We use the smallest image that is larger than the element width
			this._imgSrc = matchingSizes[0]?.url;
		}, 0);
	}

	override render() {
		const classes = {
			default: this.kind === UmbUserKind.API,
			api: this.kind === UmbUserKind.API,
			'has-image': this.hasImgUrls,
		};

		return html`<uui-avatar
			.name=${this.name || 'Unknown'}
			img-src=${ifDefined(this._imgSrc ? this._imgSrc : undefined)}
			class=${classMap(classes)}></uui-avatar>`;
	}

	static override styles = [
		css`
			uui-avatar {
				background-color: transparent;
				color: inherit;
				box-shadow: 0 0 0 1.5px var(--uui-color-border);
			}

			uui-avatar.has-image {
				border-color: transparent;
			}

			uui-avatar.api {
				border-radius: 9%;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbUserAvatarElement;
	}
}
