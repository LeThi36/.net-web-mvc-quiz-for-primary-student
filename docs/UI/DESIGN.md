---
name: Sunny Blue Education
colors:
  surface: '#f7f9fb'
  surface-dim: '#d8dadc'
  surface-bright: '#f7f9fb'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f2f4f6'
  surface-container: '#eceef0'
  surface-container-high: '#e6e8ea'
  surface-container-highest: '#e0e3e5'
  on-surface: '#191c1e'
  on-surface-variant: '#414754'
  inverse-surface: '#2d3133'
  inverse-on-surface: '#eff1f3'
  outline: '#727785'
  outline-variant: '#c2c6d6'
  surface-tint: '#005ac2'
  primary: '#0058bd'
  on-primary: '#ffffff'
  primary-container: '#1470e8'
  on-primary-container: '#fefcff'
  inverse-primary: '#adc6ff'
  secondary: '#7a5900'
  on-secondary: '#ffffff'
  secondary-container: '#fcbc05'
  on-secondary-container: '#6b4e00'
  tertiary: '#934700'
  on-tertiary: '#ffffff'
  tertiary-container: '#b95a00'
  on-tertiary-container: '#fffbff'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#d8e2ff'
  primary-fixed-dim: '#adc6ff'
  on-primary-fixed: '#001a41'
  on-primary-fixed-variant: '#004494'
  secondary-fixed: '#ffdea2'
  secondary-fixed-dim: '#fcbc05'
  on-secondary-fixed: '#261900'
  on-secondary-fixed-variant: '#5c4200'
  tertiary-fixed: '#ffdbc7'
  tertiary-fixed-dim: '#ffb688'
  on-tertiary-fixed: '#311300'
  on-tertiary-fixed-variant: '#733600'
  background: '#f7f9fb'
  on-background: '#191c1e'
  surface-variant: '#e0e3e5'
typography:
  h1:
    fontFamily: Plus Jakarta Sans
    fontSize: 40px
    fontWeight: '800'
    lineHeight: '1.2'
    letterSpacing: -0.02em
  h2:
    fontFamily: Plus Jakarta Sans
    fontSize: 32px
    fontWeight: '700'
    lineHeight: '1.2'
  h3:
    fontFamily: Plus Jakarta Sans
    fontSize: 24px
    fontWeight: '700'
    lineHeight: '1.3'
  body-lg:
    fontFamily: Lexend
    fontSize: 18px
    fontWeight: '400'
    lineHeight: '1.6'
  body-md:
    fontFamily: Lexend
    fontSize: 16px
    fontWeight: '400'
    lineHeight: '1.6'
  label-bold:
    fontFamily: Lexend
    fontSize: 14px
    fontWeight: '600'
    lineHeight: '1.2'
    letterSpacing: 0.05em
  button:
    fontFamily: Lexend
    fontSize: 18px
    fontWeight: '600'
    lineHeight: '1'
rounded:
  sm: 0.5rem
  DEFAULT: 1rem
  md: 1.5rem
  lg: 2rem
  xl: 3rem
  full: 9999px
spacing:
  base: 8px
  xs: 4px
  sm: 12px
  md: 24px
  lg: 40px
  xl: 64px
  gutter: 24px
  margin: 32px
---

## Brand & Style

The design system is built to bridge the gap between playful engagement for students and functional clarity for teachers. The personality is **fun, vibrant, and encouraging**, designed to lower the anxiety often associated with testing while maintaining a sense of safety and trust.

The visual style follows a **Tactile / Skeuomorphic-lite** approach mixed with **High-Contrast Bold** elements. This includes "squishy" buttons that feel interactive, chunky 3D-style icons that appeal to younger users, and a card-based architecture that keeps information organized and digestible. The student experience is immersive and gamified, while the teacher experience transitions into a cleaner, high-efficiency dashboard that retains the core color energy without distracting from data analysis.

## Colors

The palette is anchored by "Sunny Blue," a vibrant primary hue that conveys reliability and energy. Bright Yellow serves as the primary accent for calls-to-action and "aha!" moments.

The secondary palette is functional:
- **Playful Green (#8AC926):** Used for correct answers, progress bars, and Science subjects.
- **Energetic Orange (#FF595E):** Used for alerts, time-sensitive tasks, and Math subjects.
- **Soft Purple (#8338EC):** Used for secondary interactions, badges, and Literacy subjects.

Neutrals are kept very cool and light (#F8FAFC) to ensure the vibrant colors pop without causing visual fatigue. High contrast (WCAG AA/AAA) is prioritized for all text-on-color combinations.

## Typography

This design system utilizes **Plus Jakarta Sans** for headlines to provide a friendly, geometric, and modern feel. Its natural roundness fits the primary school aesthetic perfectly. 

For body copy and labels, **Lexend** is used. Lexend was specifically designed to reduce visual stress and improve reading fluency, making it the ideal choice for an educational platform catering to developing readers and students with diverse learning needs. All text should maintain generous line heights to ensure maximum legibility during timed quizzes.

## Layout & Spacing

The design system employs a **Fluid Grid** model with a 12-column structure for the teacher dashboard and a more focused, centered 8-column layout for student quiz views.

Spacing is based on an 8px rhythmic scale. For student interfaces, "Large" (40px) and "Extra Large" (64px) spacing is used to create a sense of airiness and reduce cognitive load. The teacher side utilizes "Small" and "Medium" spacing to increase information density and allow for complex data tables and gradebooks to be viewed without excessive scrolling.

## Elevation & Depth

Visual hierarchy is established using **Tonal Layers** and **Ambient Shadows** with a distinct color tint. 

- **Cards:** Use a soft, 12% opacity shadow tinted with the primary blue (#3A86FF) rather than pure black. This keeps the interface feeling "clean" and "airy."
- **Buttons:** Utilize a "pseudo-3D" effect. A button has a solid 4px bottom border in a darker shade of its own color, creating a tactile "pushable" feel.
- **Floating Elements:** Modals and pop-overs use a deep, diffused shadow to clearly separate them from the workspace.
- **Teacher Dashboard:** Uses low-contrast outlines (1px solid #E2E8F0) to separate data sections, reserving shadows only for the most important interactive cards.

## Shapes

The design system uses a **Pill-shaped (3)** roundedness philosophy. Sharp corners are avoided entirely to maintain a friendly and safe environment. 

Large containers like cards should use `rounded-xl` (3rem/48px), while buttons and input fields use full pill-shapes. This extreme roundness conveys a "toy-like" approachability for students while appearing modern and "soft-tech" for teachers.

## Components

### Buttons
Buttons are large and chunky. The **Primary Button** uses Sunny Blue with a darker blue "depth" border (4px). On click, the button should shift down 2px to simulate a physical press.

### Chips & Subject Tags
Small, pill-shaped tags used for subject categorization (e.g., "Math", "Science"). These use high-saturation background colors with white text for maximum contrast.

### Cards
Student cards are highly illustrative, featuring large avatars or badge icons. Teacher cards are more streamlined, using a top-border accent color to denote category without overwhelming the data inside.

### Form Inputs
Inputs use a thick 2px border and a large font size (18px). When focused, the border transitions to Sunny Blue with a soft outer glow.

### Additional Educational Components
- **Progress Trackers:** Thick, rounded bars with a "pulsing" animation on the leading edge.
- **Reward Badges:** Circular 3D containers with a gold or silver sheen for achievements.
- **Teacher Gradebook:** A condensed list view with color-coded "performance dots" for quick scanning.