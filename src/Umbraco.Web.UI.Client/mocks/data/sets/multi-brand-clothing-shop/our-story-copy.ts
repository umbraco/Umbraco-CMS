import type { UmbMbcsSiteCode } from './catalog.js';

interface UmbMbcsStorySection {
	headline: string;
	paragraphs: Array<string>;
	caption: string;
}

export interface UmbMbcsOurStoryCopy {
	heroSubheadline: string;
	intro: { headline: string; paragraphs: Array<string> };
	beginning: UmbMbcsStorySection;
	design: UmbMbcsStorySection;
	timeline: { headline: string; lead: string; entries: Array<{ year: string; text: string }> };
	makers: UmbMbcsStorySection;
	values: { headline: string; paragraphs: Array<string> };
	productsLead: { headline: string; text: string };
	articlesLead: { headline: string; text: string };
	closing: { headline: string; paragraphs: Array<string> };
}

const littleOnes: UmbMbcsOurStoryCopy = {
	heroSubheadline: 'Ten years of making clothes for children who never sit still.',
	intro: {
		headline: 'Clothes that keep up with childhood',
		paragraphs: [
			'Little Ones began at a kitchen table in Copenhagen, with two parents, a pile of outgrown bodysuits and one stubborn question: why do children’s clothes so often fall apart long before children grow out of them?',
			'Ten years later, the question is still how we start every collection. We design for the way children actually live: climbing, crawling, splashing in puddles and falling asleep in the car. Everything we make has to be soft enough for the first week and strong enough for the third sibling.',
		],
	},
	beginning: {
		headline: 'How it started',
		caption: 'Where it all began',
		paragraphs: [
			'The first Little Ones collection was twelve pieces, made in small batches and sold from a simple webshop. There was no marketing budget, only parents telling other parents that the clothes were comfortable, easy to wash and still looked good after a winter of daycare.',
			'That word of mouth shaped everything that followed. We learned to listen: to the tired parent who needs a zip that works with one hand, to the toddler who refuses anything scratchy, and to the grandparent who just wants something that feels special.',
		],
	},
	design: {
		headline: 'Designed for real life',
		caption: 'Testing a new knit with our youngest panel member',
		paragraphs: [
			'Every design starts with a drawing, but it is finished by children. Our kids’ panel wears each sample for weeks before it goes into production, and their verdict is honest: if it is itchy, too tight or impossible to put on, it does not make the cut.',
			'We keep the shapes simple and the details useful. Flat seams, generous necklines, adjustable waistbands and room to grow are not features we add at the end; they are where the design begins.',
			'The prints are drawn by hand by illustrators we know personally, so the foxes, dinosaurs and stripes you see in the shop are never just a pattern from a catalogue.',
		],
	},
	timeline: {
		headline: 'Ten years, in short',
		lead: 'A few of the moments that made Little Ones what it is today.',
		entries: [
			{ year: '2016', text: 'Little Ones is founded in Copenhagen with a first collection of twelve pieces.' },
			{ year: '2018', text: 'We move to organic cotton for all our everyday basics.' },
			{ year: '2019', text: 'The first kids’ panel is formed, and testing becomes part of every design.' },
			{ year: '2021', text: 'Our first store opens in Copenhagen.' },
			{ year: '2023', text: 'The take-back programme launches, giving outgrown clothes a second life.' },
			{ year: '2024', text: 'We open in Hamburg, followed by Odense a year later.' },
			{ year: '2026', text: 'Ten years of Little Ones, and still just getting started.' },
		],
	},
	makers: {
		headline: 'Made with people we know',
		caption: 'A visit to one of our knitwear partners',
		paragraphs: [
			'We work with a small number of partners and visit them regularly. Our knitwear comes from a family-run mill in Portugal, our woven pieces from workshops we have worked with for years, and our prams suits are finished by local seamstresses.',
			'Long relationships mean we can talk openly about quality, working conditions and what can be improved. Our supplier code of conduct is public, and we would rather change a partner’s process together than simply walk away.',
		],
	},
	values: {
		headline: 'What we stand for',
		paragraphs: [
			'Quality over quantity. We make fewer styles and make them better, so that clothes can be passed on from child to child rather than ending up at the back of a drawer.',
			'Honesty about materials. Every product page tells you what the garment is made of and how to look after it, because good care is the cheapest way to make clothes last.',
			'Responsibility, step by step. We do not claim to be perfect. We set clear goals, report on our progress and keep improving, from plastic-free packaging to our decision not to take part in Black Friday.',
		],
	},
	productsLead: {
		headline: 'Pieces that carry our story',
		text: 'A few of the clothes that best show what Little Ones is about.',
	},
	articlesLead: {
		headline: 'From behind the seams',
		text: 'Read more about the people, places and processes behind our clothes.',
	},
	closing: {
		headline: 'Come and say hello',
		paragraphs: [
			'The best way to get to know Little Ones is to pick something up and feel it. You can find us in our stores in Copenhagen, Hamburg and Odense, where the team is always happy to help with sizes, gifts and questions about care.',
			'Cannot make it in? Our customer service team is only a message away, and we answer every one.',
		],
	},
};

const outdoorShop: UmbMbcsOurStoryCopy = {
	heroSubheadline: 'A decade of clothes made to be worn, repaired and worn again.',
	intro: {
		headline: 'Built for the weather, made for the long run',
		paragraphs: [
			'The Outdoor Shop started with a jacket. It was a good jacket, but after two winters the zip broke, the lining wore through and nobody could tell us how to fix it. We decided there had to be a better way to make, and look after, the things we wear.',
			'A decade on, that idea runs through everything we do. We make clothes for the Danish climate: wind off the coast, rain that comes sideways and long evenings that call for a good sweater. And we stand behind them with a repair service and a lifetime guarantee.',
		],
	},
	beginning: {
		headline: 'How it started',
		caption: 'The first collection, laid out for the lookbook',
		paragraphs: [
			'The first collection was small: a handful of knits, two jackets and a rain shell. We tested everything ourselves on hikes, bike commutes and weekend trips, and rewrote the designs every time something fell short.',
			'Early customers became our best critics. They told us which pockets were in the wrong place, which wool pilled and which colours they actually wore. Many of those conversations are still visible in today’s designs.',
		],
	},
	design: {
		headline: 'Design with restraint',
		caption: 'Fitting a new overcoat in the studio',
		paragraphs: [
			'We design in a Scandinavian spirit: simple lines, a limited palette and details that earn their place. A pocket is where your hands want it, a collar sits the way it should and nothing is there just for show.',
			'Colours are chosen to work together, so a sweater from last winter still goes with a coat from this one. That is how a wardrobe gets smaller and better at the same time.',
			'Each piece is tested for durability before it reaches the shop, from abrasion and seam strength to how a fabric behaves after fifty washes.',
		],
	},
	timeline: {
		headline: 'Ten years, in short',
		lead: 'A few of the moments that shaped The Outdoor Shop.',
		entries: [
			{ year: '2016', text: 'The Outdoor Shop is founded with a first collection of knitwear and outerwear.' },
			{ year: '2018', text: 'Our repair studio opens, and the lifetime guarantee is introduced.' },
			{ year: '2019', text: 'We start using recycled and certified materials across the range.' },
			{ year: '2021', text: 'Our first store opens in Hamburg.' },
			{ year: '2023', text: 'The first lookbook is shot on location along the Danish coast.' },
			{ year: '2025', text: 'We open stores in Copenhagen and Aalborg.' },
			{ year: '2026', text: 'Ten years in, with a repair studio that has mended thousands of garments.' },
		],
	},
	makers: {
		headline: 'Made by people we trust',
		caption: 'Inside one of our knitwear partners',
		paragraphs: [
			'Our knitwear is made in a family-run mill in Portugal, our outerwear in factories we have visited and audited, and our leather goods by craftspeople who have worked with the material for generations.',
			'We publish our supplier code of conduct and work with our partners over many years, because long relationships are the surest way to good quality and fair conditions.',
		],
	},
	values: {
		headline: 'What we stand for',
		paragraphs: [
			'Buy less, buy better. We would rather you own a few things you love and wear for years than a wardrobe you never finish.',
			'Repair before replace. Our repair studio, care guides and lifetime guarantee exist so that your clothes keep going, long after the first season.',
			'Open about what we do. From materials to certifications, we explain our choices and report on our progress, including where we still have work to do.',
		],
	},
	productsLead: {
		headline: 'Pieces that carry our story',
		text: 'A few of the clothes that best show what The Outdoor Shop is about.',
	},
	articlesLead: {
		headline: 'From behind the seams',
		text: 'Meet the people and see the processes behind what we make.',
	},
	closing: {
		headline: 'Come and say hello',
		paragraphs: [
			'The best way to understand our clothes is to try them on. Visit us in Copenhagen, Hamburg or Aalborg, where the team can help you find the right fit, or tell you how to give an old favourite a longer life.',
			'If you cannot visit, our customer service team is only a message away.',
		],
	},
};

export const getOurStoryCopy = (site: UmbMbcsSiteCode): UmbMbcsOurStoryCopy =>
	site === 'LO' ? littleOnes : outdoorShop;
