import '../backoffice/components/backoffice-modal-container.element';
import '../core/services/modal/layouts/content-picker/modal-layout-content-picker.element';
import '../core/context/context-provider.element';
import '../backoffice/editors/shared/editor-layout/editor-layout.element';
import '../backoffice/components/icon-selector.element';

import '@umbraco-ui/uui-modal';
import '@umbraco-ui/uui-modal-container';
import '@umbraco-ui/uui-modal-sidebar';
import '@umbraco-ui/uui-modal-dialog';

import { Meta, Story } from '@storybook/web-components';
import { customElement, property, state } from 'lit/decorators.js';
import { LitElement } from 'lit';
import { html } from 'lit-html';
import { UmbModalService } from '../core/services/modal';
import { UmbContextConsumerMixin } from '../core/context';

export default {
	title: 'Icon Selector',
	component: 'umb-icon-selector',
	id: 'icon-selector',
	decorators: [
		(story) =>
			html`<umb-context-provider
				style="display: block; padding: 32px;"
				key="umbModalService"
				.value=${new UmbModalService()}>
				${story()}
			</umb-context-provider>`,
	],
} as Meta;

@customElement('story-modal-icon-selector')
class StoryModalIconSelector extends UmbContextConsumerMixin(LitElement) {
	@state()
	value = '';

	private _modalService?: UmbModalService;

	constructor() {
		super();
		this.consumeContext('umbModalService', (modalService: UmbModalService) => {
			this._modalService = modalService;
		});
	}

	private _openModal() {
		this._modalService?.open('umb-icon-selector', { type: 'sidebar', size: 'small' });
	}

	render() {
		return html`
			<uui-button label="open-dialog" look="secondary" @click=${() => this._openModal()} style="margin-right: 9px;"
				>Pick an icon</uui-button
			>
		`;
	}
}

const Template: Story = () => {
	return html`<umb-backoffice-modal-container></umb-backoffice-modal-container>
		<story-modal-icon-selector></story-modal-icon-selector> `;
};

export const IconSelectorModal = Template.bind({});

export const IconSelector = () => html`<umb-icon-selector></umb-icon-selector>`;
