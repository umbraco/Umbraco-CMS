import { Meta, StoryObj } from '@storybook/web-components';
import { html } from 'lit-html';
import './code-block.element';
import type { UUICodeBlock } from './code-block.element';

const meta: Meta<UUICodeBlock> = {
    title: 'Components/Code Block',
    component: 'uui-code-block'
};
  
export default meta;
type Story = StoryObj<UUICodeBlock>;
  
export const Overview: Story = {
    args: {
    }
};

export const WithCode: Story = {
    decorators: [],
    render: () => html`
        <uui-code-block>
            // Lets write some javascript
            alert("Hello World");
        </uui-code-block>`
};