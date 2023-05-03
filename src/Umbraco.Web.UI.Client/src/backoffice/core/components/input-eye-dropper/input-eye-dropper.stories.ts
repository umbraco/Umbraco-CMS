import { Meta, StoryObj } from '@storybook/web-components';
import './input-eye-dropper.element';
import type { UmbInputEyeDropperElement } from './input-eye-dropper.element';

const meta: Meta<UmbInputEyeDropperElement> = {
    title: 'Components/Inputs/Eye Dropper',
    component: 'umb-input-eye-dropper'
};
  
export default meta;
type Story = StoryObj<UmbInputEyeDropperElement>;
  
export const Overview: Story = {
    args: {
        
    }
};

export const WithOpacity: Story = {
    args: {
        opacity: true,
    }
};

export const WithSwatches: Story = {
    args: {
        swatches: ['#000000', '#ffffff', '#ff0000', '#00ff00', '#0000ff']
    }
};
