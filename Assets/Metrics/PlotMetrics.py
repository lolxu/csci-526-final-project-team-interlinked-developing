import json
import matplotlib.pyplot as plt
import seaborn as sns
import numpy as np
import pandas as pd
from matplotlib.figure import Figure
from matplotlib.backends.backend_agg import FigureCanvasAgg as FigureCanvas

plt.style.use('seaborn-v0_8-whitegrid')
sns.set_palette("husl")

# Parsing JSON...
with open('interlink-metrics-default-rtdb-export.json', 'r') as file:
    data = json.load(file)

# Extract all unique level names since we do have a lot
levels = set()
for session_id, session_data in data.items():
    if 'm_levelMetricsData' in session_data:
        for level in session_data['m_levelMetricsData']:
            if 'm_levelName' in level:
                levels.add(level['m_levelName'])

print(f"Found {len(levels)} unique levels: {', '.join(levels)}")

# Rope connection/disconnection metrics by level
level_data = {}

# Initialize data structure for each level
for level_name in levels:
    level_data[level_name] = {
        'rope_connect_by_wave': {},
        'rope_disconnect_by_wave': {},
        'max_wave_count': 0
    }

# Process data
for session_id, session_data in data.items():
    if 'm_levelMetricsData' in session_data:
        for level in session_data['m_levelMetricsData']:
            level_name = level.get('m_levelName')
            if level_name and 'm_ropeConnectionMetrics' in level and 'm_ropeDisconnectionMetrics' in level:
                connects = level['m_ropeConnectionMetrics']
                disconnects = level['m_ropeDisconnectionMetrics']
                level_stats = level_data[level_name]
                
                # Update max wave count for this level
                level_stats['max_wave_count'] = max(level_stats['max_wave_count'], len(connects))
                
                for wave_index, count in enumerate(connects):
                    wave_name = f'Wave {wave_index+1}'
                    if wave_name not in level_stats['rope_connect_by_wave']:
                        level_stats['rope_connect_by_wave'][wave_name] = []
                    if count > 0:  # Only add non-zero data
                        level_stats['rope_connect_by_wave'][wave_name].append(count)
                
                # Add data points for disconnections
                for wave_index, count in enumerate(disconnects):
                    wave_name = f'Wave {wave_index+1}'
                    if wave_name not in level_stats['rope_disconnect_by_wave']:
                        level_stats['rope_disconnect_by_wave'][wave_name] = []
                    if count > 0:  # Only add non-zero data
                        level_stats['rope_disconnect_by_wave'][wave_name].append(count)

# 3. Process weapon steal rates
weapon_steal_rates = {}

for session_id, session_data in data.items():
    if 'm_weaponMetricsData' in session_data:
        for weapon in session_data['m_weaponMetricsData']:
            if 'm_weaponName' in weapon and 'm_stealRate' in weapon:
                weapon_name = weapon['m_weaponName']
                if weapon_name not in weapon_steal_rates:
                    weapon_steal_rates[weapon_name] = []
                # Include all data, even zeros since we can have some 0 rates...
                weapon_steal_rates[weapon_name].append(weapon['m_stealRate'])

# 4. Process ability activation rates
ability_activation_rates = {}

for session_id, session_data in data.items():
    if 'm_abilityMetricsData' in session_data:
        for ability in session_data['m_abilityMetricsData']:
            if 'm_abilityName' in ability and 'm_activationRate' in ability:
                ability_name = ability['m_abilityName']
                if ability_name not in ability_activation_rates:
                    ability_activation_rates[ability_name] = []
                # Include all data, even zeros since we can have some 0 rates...
                ability_activation_rates[ability_name].append(ability['m_activationRate'])

# Create visualizations for rope operations...
for level_name, stats in level_data.items():
      # Only create visualizations for levels with rope data, so we ignore main menu
    if stats['max_wave_count'] > 0:
        print(f"Creating rope metrics visualization for {level_name}...")
        
        # Prepare data for box plot
        connect_data = []
        disconnect_data = []
        wave_order = sorted([w for w in stats['rope_connect_by_wave'].keys() if stats['rope_connect_by_wave'][w]], 
                           key=lambda x: int(x.split(' ')[1]))
        
        for wave in wave_order:
            for count in stats['rope_connect_by_wave'].get(wave, []):
                connect_data.append({'Wave': wave, 'Count': count, 'Type': 'Connections'})
            for count in stats['rope_disconnect_by_wave'].get(wave, []):
                disconnect_data.append({'Wave': wave, 'Count': count, 'Type': 'Disconnections'})
        
        # Skip if no data to plot
        if not connect_data and not disconnect_data:
            print(f"  No rope data to plot for {level_name}")
            continue
        
        # Combine the data
        rope_data = pd.DataFrame(connect_data + disconnect_data)
        
        # Create the plot
        fig, ax = plt.subplots(figsize=(12, 8))
        
        sns.boxplot(x='Wave', y='Count', hue='Type', data=rope_data, ax=ax)
        ax.set_title(f'Rope Connections and Disconnections by Wave - {level_name}', fontsize=16)
        ax.set_xlabel('Wave', fontsize=14)
        ax.set_ylabel('Count', fontsize=14)
        ax.legend(title='Metric Type')
        
        # Adjust layout
        plt.tight_layout()
        plt.savefig(f'rope_metrics_{level_name.replace(" ", "_")}.png', dpi=300)
        plt.close()

# Visualization for weapon steal rates
print("Creating weapon steal rates visualization...")
fig2, ax2 = plt.subplots(figsize=(10, 6))

# Prepare data for box plot
weapon_data = []
for weapon, rates in weapon_steal_rates.items():
    if rates:  # Only include weapons with data
        for rate in rates:
            weapon_data.append({'Weapon': weapon, 'Steal Rate': rate})

# Create DataFrame
weapon_df = pd.DataFrame(weapon_data)

# Create the plot
if len(weapon_df) > 0:
    sns.boxplot(x='Weapon', y='Steal Rate', data=weapon_df, ax=ax2, showfliers=True)
    ax2.set_title('Weapon Steal Rates', fontsize=16)
    ax2.set_xlabel('Weapon', fontsize=14)
    ax2.set_ylabel('Steal Rate', fontsize=14)
    
    # Add count annotations above each box for better clarity
    for i, weapon in enumerate(weapon_steal_rates.keys()):
        count = len(weapon_steal_rates[weapon])
        avg = sum(weapon_steal_rates[weapon]) / count if count > 0 else 0
        ax2.annotate(f'n={count}\navg={avg:.2f}', 
                    xy=(i, 0.02), 
                    xytext=(0, 10),
                    textcoords='offset points',
                    ha='center', va='bottom')
else:
    ax2.text(0.5, 0.5, 'No weapon steal rate data available', 
             horizontalalignment='center', verticalalignment='center', 
             transform=ax2.transAxes, fontsize=14)

# Adjust layout
plt.tight_layout()
plt.savefig('weapon_steal_rates_boxplot.png', dpi=300)
plt.close()

# Activation rates
print("Creating ability activation rates visualization...")
fig3, ax3 = plt.subplots(figsize=(10, 6))

# Prepare data for box plot
ability_data = []
for ability, rates in ability_activation_rates.items():
    if rates:  # Only include abilities with data
        for rate in rates:
            ability_data.append({'Ability': ability, 'Activation Rate': rate})

# Create DataFrame
ability_df = pd.DataFrame(ability_data)

# Create the plot
if len(ability_df) > 0:
    sns.boxplot(x='Ability', y='Activation Rate', data=ability_df, ax=ax3, showfliers=True)
    ax3.set_title('Ability Activation Rates', fontsize=16)
    ax3.set_xlabel('Ability', fontsize=14)
    ax3.set_ylabel('Activation Rate', fontsize=14)
    
    # Add count annotations above each box for better clarity
    for i, ability in enumerate(ability_activation_rates.keys()):
        count = len(ability_activation_rates[ability])
        avg = sum(ability_activation_rates[ability]) / count if count > 0 else 0
        ax3.annotate(f'n={count}\navg={avg:.2f}', 
                    xy=(i, 0.02), 
                    xytext=(0, 10),
                    textcoords='offset points',
                    ha='center', va='bottom')
else:
    ax3.text(0.5, 0.5, 'No ability activation rate data available', 
             horizontalalignment='center', verticalalignment='center', 
             transform=ax3.transAxes, fontsize=14)

# Adjust layout
plt.tight_layout()
plt.savefig('ability_activation_rates_boxplot.png', dpi=300)
plt.close()

print("Visualizations completed!")