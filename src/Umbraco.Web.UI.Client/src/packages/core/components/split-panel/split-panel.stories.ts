import type { Meta, StoryObj } from '@storybook/web-components';
import './split-panel.element.js';
import type { UmbSplitPanelElement } from './split-panel.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

const meta: Meta<UmbSplitPanelElement> = {
	title: 'Components/Split Panel',
	component: 'umb-split-panel',
	argTypes: {
		lock: { options: ['none', 'start', 'end'] },
		snap: { control: 'text' },
		position: { control: 'text' },
	},
	args: {
		lock: 'start',
		snap: '',
		position: '50%',
	},
};

export default meta;
type Story = StoryObj<UmbSplitPanelElement>;

export const Overview: Story = {
	render: (props) => html`
		<umb-split-panel .lock=${props.lock} .snap=${props.snap} .position=${props.position}>
			<div id="start" slot="start">Start</div>
			<div id="end" slot="end">End</div>
		</umb-split-panel>
		<style>
			#start,
			#end {
				background-color: #ffffff;
				color: #383838;
				display: flex;
				align-items: center;
				justify-content: center;
				font-size: 1.5rem;
				font-weight: 600;
			}
			#start {
				border-right: 2px solid #e5e5e5;
				min-height: 300px;
			}
			#end {
				border-left: 2px solid #e5e5e5;
			}
		</style>
	`,
};
