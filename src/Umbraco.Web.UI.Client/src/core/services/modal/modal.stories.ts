import { Meta, Story } from '@storybook/web-components';
import { LitElement } from 'lit';
import { html } from 'lit-html';
import { customElement, property, state } from 'lit/decorators.js';

import { UmbModalService } from './';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';

export default {
	title: 'API/Modals',
	id: 'umb-modal-service',
	argTypes: {
		modalLayout: {
			control: 'select',
			options: ['Confirm', 'Content Picker', 'Property Editor UI Picker', 'Icon Picker'],
		},
	},
} as Meta;

@customElement('story-modal-service-example')
export class StoryModalServiceExampleElement extends UmbContextConsumerMixin(LitElement) {
	@property()
	modalLayout = 'confirm';

	@state()
	value = '';

	private _modalService?: UmbModalService;

	constructor() {
		super();
		this.consumeContext('umbModalService', (modalService: UmbModalService) => {
			this._modalService = modalService;
		});
	}

	private _open() {
		switch (this.modalLayout) {
			case 'Content Picker':
				this._modalService?.contentPicker();
				break;
			case 'Property Editor UI Picker':
				this._modalService?.propertyEditorUIPicker();
				break;
			case 'Icon Picker':
				this._modalService?.iconPicker();
				break;
			default:
				this._modalService?.confirm({
					headline: 'Headline',
					content: 'Lorem ipsum dolor sit amet, consectetur adipiscing elit',
				});
				break;
		}
	}

	render() {
		return html`
			<uui-button label="open-dialog" look="primary" @click=${() => this._open()} style="margin-right: 9px;"
				>Open modal</uui-button
			>
		`;
	}
}

const Template: Story = (props) => {
	return html` <story-modal-service-example .modalLayout=${props.modalLayout}></story-modal-service-example> `;
};

export const Overview = Template.bind({});
