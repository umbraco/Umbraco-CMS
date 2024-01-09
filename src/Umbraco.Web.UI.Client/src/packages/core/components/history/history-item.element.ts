import { css, html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-history-item')
export class UmbHistoryItemElement extends UmbLitElement {
	@property({ type: String })
	src?: string;

	@property({ type: String })
	name?: string;

	@property({ type: String })
	detail?: string;

	render() {
		return html`
			<div class="user-info">
				<uui-avatar .name="${this.name ?? 'Unknown'}" ?src="${this.src}"></uui-avatar>
				<div>
					<span class="name">${this.name}</span>
					<span class="detail">${this.detail}</span>
				</div>
			</div>
			<slot id="description"></slot>
			<slot id="actions-container" name="actions"></slot>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				--avatar-size: calc(2em + 4px);
				display: contents;
			}

			slot[name='actions'] {
				--uui-button-border-radius: 50px 50px 50px 50px;
				display: flex;
				align-items: center;
				--uui-button-height: calc(var(--uui-size-2) * 4);
				margin-right: var(--uui-size-2);
			}
			#actions-container {
				opacity: 0;
				transition: opacity 120ms;
			}
			:host(:hover) #actions-container {
				opacity: 1;
			}

			:host(:hover) #actions-container {
				opacity: 1;
			}

			.user-info {
				position: relative;
				display: flex;
				align-items: flex-end;
				gap: var(--uui-size-space-5);
			}

			.user-info div {
				display: flex;
				flex-direction: column;
			}

			.detail {
				font-size: var(--uui-size-4);
				color: var(--uui-color-text-alt);
				line-height: 1;
			}
		`,
	];
}

export default UmbHistoryItemElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-history-item': UmbHistoryItemElement;
	}
}
